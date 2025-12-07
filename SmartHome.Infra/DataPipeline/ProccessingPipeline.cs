using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using SmartHome.Core.Domain;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SmartHome.Infra.DataPipeline
{
    public class ProcessingPipeline : IDataProcessingPipeline
    {
        private BufferBlock<string> _inputBuffer; // Работаем с байтами вместо строк

        private TransformBlock<string, SensorMessage> _parsingBlock;
        private TransformManyBlock<SensorMessage, ProcessedData> _validationBlock;
        
        private BatchBlock<ProcessedData> _batchingBlock;
        private ActionBlock<ProcessedData[]> _databaseWriterBlock;

        // базовые настройки 
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ProcessingPipeline> _logger;
        private readonly ICache _cache; // теперь используем ICache

        // Пул для ProcessedData объектов (Object Pool pattern)
        private static readonly ObjectPool<ProcessedData> _processedDataPool =
            new DefaultObjectPool<ProcessedData>(new ProcessedDataPooledPolicy(), 1000);

        public ProcessingPipeline(
            ILogger<ProcessingPipeline> logger,
            IServiceScopeFactory serviceScopeFactory,
            ICache cache = null
            )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _cache = cache;
            _logger.LogInformation("ProcessingPipeline initialized");
            CreatePipelineBlocks();
            LinkPipelineBlocks();
        }

        private void CreatePipelineBlocks()
        {
            // Буфер для входящих данных
            _inputBuffer = new BufferBlock<string>(new DataflowBlockOptions
            {
                BoundedCapacity = 10000,
            });

            // Блок Парсинга JSON
            _parsingBlock = new TransformBlock<string, SensorMessage>(jsonString =>
            {
                try
                {
                    return OptimizedJsonParser.ParseFromString(jsonString);
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"ПАРСИНГ: ОШИБКА JSON - {ex.Message}");
                    _logger.LogError($"ПАРСИНГ: Проблемный JSON: {jsonString}");
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ПАРСИНГ: НЕИЗВЕСТНАЯ ОШИБКА - {ex.Message}");
                    return null;
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 8,
                BoundedCapacity = 1000
            });

            // Блок валидации
            _validationBlock = new TransformManyBlock<SensorMessage, ProcessedData>(sensorMessage =>
            {
                if (sensorMessage?.Messages == null || sensorMessage.Messages.Count == 0)
                {
                    _logger.LogError($"ВАЛИДАЦИЯ: ОШИБКА - нет сообщений или Messages=null");
                    return Enumerable.Empty<ProcessedData>();
                }

                var processedData = new List<ProcessedData>(7);

                foreach (var message in sensorMessage.Messages)
                {
                    if (message.Payload != null)
                    {
                        var payload = message.Payload;
                        AddIfHasValue(processedData, "Humanidity", message.Timestamp, payload.Humanidity);
                        AddIfHasValue(processedData, "Temperature", message.Timestamp, payload.Temperature);
                        AddIfHasValue(processedData, "Pressure", message.Timestamp, payload.Pressure);
                        AddIfHasValue(processedData, "Motion", message.Timestamp, payload.Motion);
                        AddIfHasValue(processedData, "Light", message.Timestamp, payload.Light);
                        AddIfHasValue(processedData, "Smokedetektor", message.Timestamp, payload.Smokedetector);
                    }
                }
                _logger.LogDebug($"ВАЛИДАЦИЯ: УСПЕХ - создано {processedData.Count}");
                return processedData;
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 8,
                BoundedCapacity = 1000
            });

            // Блок пакетной обработки (группируем по 100 сообщений)
            _batchingBlock = new BatchBlock<ProcessedData>(100);

            // Финальный блок для записи в базу
            _databaseWriterBlock = new ActionBlock<ProcessedData[]>(async batch =>
            {
                await WriteBatchToDatabase(batch.Where(x => x != null).ToArray());
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 4
            });
        }

        // Соединяем блоки
        private void LinkPipelineBlocks()
        {
            _inputBuffer.LinkTo(_parsingBlock);
            _parsingBlock.LinkTo(_validationBlock);
            _validationBlock.LinkTo(_batchingBlock);
            _batchingBlock.LinkTo(_databaseWriterBlock);

            // Настраиваем пропуск невалидных сообщений
            _parsingBlock.LinkTo(DataflowBlock.NullTarget<SensorMessage>(), msg => msg == null);
            _validationBlock.LinkTo(DataflowBlock.NullTarget<ProcessedData>(), msg => msg == null);
        }

        private async Task WriteBatchToDatabase(ProcessedData[] batch)
        {
            try
            {
                var telemetrydata = batch
                    .Where(x => x.Value.HasValue)
                    .Select(data => new SensorTelemetry
                    {
                        SensorId = data.SensorId,
                        Time = data.Timestamp,
                        Value = data.Value.Value
                    });

                var uniqueSensors = batch
                    .Where(x => !string.IsNullOrEmpty(x.SensorId))
                    .GroupBy(x => x.SensorId)
                    .Select(g => new Sensor
                    {
                        SensorId = g.Key,
                    })
                    .ToList();

                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISensorTelemetryRepository>();
                var sensorrepository = scope.ServiceProvider.GetRequiredService<ISensorsRepository>();

                await repository.BatchInsertAsync(telemetrydata.ToList());
                await sensorrepository.BatchInsertAsync(uniqueSensors);

                // Используем ICache через адаптер ICache.UpdateCache (не блокируем запись в БД)
                await UpdateCacheUsingICacheAsync(telemetrydata.ToArray());

                _logger.LogInformation(
                    $"Записано {telemetrydata.ToArray().Length} телеметрий, " +
                    $"обновлено {uniqueSensors.Count} датчиков");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data");
                throw;
            }
        }

        // Обёртка для интеграции с ICache.
        // ICache.UpdateCache — синхронный метод в интерфейсе, поэтому вызываем его быстро и безопасно.
        private Task UpdateCacheUsingICacheAsync(SensorTelemetry[] telemetryData)
        {
            if (_cache == null || telemetryData == null || telemetryData.Length == 0)
            {
                return Task.CompletedTask;
            }

            try
            {
                foreach (var data in telemetryData)
                {
                    try
                    {
                        _cache.UpdateCache(data);
                    }
                    catch (Exception ex)
                    {
                        // Локально логируем проблему с конкретной записью и продолжаем
                        _logger.LogWarning(ex, "ICache.UpdateCache failed for sensor {SensorId}", data.SensorId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache update via ICache failed but continuing");
            }

            return Task.CompletedTask;
        }

        // Метод для добавления данных с использованием пула объектов
        private void AddIfHasValue(List<ProcessedData> list, string sensorId,
                                   DateTime timestamp, double? value)
        {
            if (value.HasValue)
            {
                var data = _processedDataPool.Get();
                data.SensorId = sensorId;
                data.Timestamp = timestamp;
                data.Value = value.Value;
                list.Add(data);
            }
        }

        public async Task ProcessDataAsync(string data)
        {
            _logger.LogDebug("ИДЕТ в конвейер");
            await _inputBuffer.SendAsync(data);
        }

        public async Task CompleteAsync()
        {
            _inputBuffer.Complete();
            await _databaseWriterBlock.Completion;
        }
    }
}
//Проверка: Напишите тест, который отправляет 10,000 сообщений в конвейер
// и замеряет время обработки.