using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartHome.Core.Domain;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartHome.Infra.DataPipeline
{
    public class ProcessingPipeline : IDataProcessingPipeline
    {
        private BufferBlock<string> _inputBuffer;
        private TransformBlock<string, SensorMessage> _parsingBlock;

        private TransformManyBlock<SensorMessage, ProcessedData> _validationBlock;
        //private TransformBlock<PayloadData, PayloadData> _enrichmentBlock;

        private BatchBlock<ProcessedData> _batchingBlock;
        private ActionBlock<ProcessedData[]> _databaseWriterBlock;

       

        // базовые настройки 
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ProcessingPipeline> _logger;
        //private ISensorTelemetryRepository _repository;

        public ProcessingPipeline(
            ILogger<ProcessingPipeline> logger,
            IServiceScopeFactory serviceScopeFactory
            )
        //ISensorTelemetryRepository repository IServiceScopeFactory это с внешней библиотеки
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            //_repository = repository;
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
            //сообщения приходят в формате JSON нужно их распарсить
            //Блок Парсинга JSON
            _parsingBlock = new TransformBlock<string, SensorMessage>(jsonString =>
            {
                try
                {
                    Console.WriteLine("ПОЛУЧЕНО СООБЩЕНИЕ ДЛЯ ПАРСИНГА");
                    var message = JsonSerializer.Deserialize<SensorMessage>(jsonString);
                    if (message == null)
                    {
                        _logger.LogDebug("ПАРСИНГ: Получен null после десериализации");
                        return null;
                    }

                    // Логируем успешный парсинг
                    _logger.LogDebug($"ПАРСИНГ: УСПЕХ - MessageId={message.MessageId}, " +
                                    $"Messages count={message.Messages?.Count ?? 0}");
                    return message;
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
                    // _logger.LogWarning(ex, "JSON parsing failed");
                    return null;
                }
            }, new ExecutionDataflowBlockOptions
            {
               MaxDegreeOfParallelism = 8,// _settings.MaxDegreeOfParallelism
               BoundedCapacity = 1000
            }
            );


            // Блок валидации (параллельная обработка) из 1 JSON несколько объектов ProcessedData 
            _validationBlock = new TransformManyBlock<SensorMessage, ProcessedData>(sensorMessage =>
            {
                if (sensorMessage?.Messages == null || sensorMessage.Messages.Count == 0)
                {
                    _logger.LogError($"ВАЛИДАЦИЯ: ОШИБКА - нет сообщений или Messages=null");
                    return Enumerable.Empty<ProcessedData>();
                }

                var processedData = new List<ProcessedData>();
                foreach (var message in sensorMessage.Messages)
                {
                    if (message.Payload != null)
                    {
                        // Явно обрабатываем каждый датчик
                        var payload = message.Payload;
                        //payload.Add(message.Payload);

                        //foreach (var data in payload)
                        //{
                        //    processedData.Add(new ProcessedData
                        //    {
                        //        SensorId = $"Sensor_{data.Humanidity}",
                        //        Timestamp = message.Timestamp,
                        //        Values = data.Humanidity.Value
                        //    });

                        //}
                        if (payload.Humanidity.HasValue)
                        {
                            processedData.Add(new ProcessedData
                            {
                                SensorId = "Humanidity",
                                Timestamp = message.Timestamp,
                                Value = payload.Humanidity.Value
                            });
                        }

                        if (payload.Temperature.HasValue)
                        {
                            processedData.Add(new ProcessedData
                            {
                                SensorId = "Temperature",
                                Timestamp = message.Timestamp,
                                Value = payload.Temperature.Value
                            });
                        }

                        if (payload.Pressure.HasValue)
                        {
                            processedData.Add(new ProcessedData
                            {
                                SensorId = "Pressure",
                                Timestamp = message.Timestamp,
                                Value = payload.Pressure.Value
                            });
                        }

                        // Добавьте остальные датчики по аналогии...
                        if (payload.Motion.HasValue)
                        {
                            processedData.Add(new ProcessedData
                            {
                                SensorId = "Motion",
                                Timestamp = message.Timestamp,
                                Value = payload.Motion.Value
                            });
                        }
                        if (payload.Light.HasValue)
                        {
                            processedData.Add(new ProcessedData
                            {
                                SensorId = "Light",
                                Timestamp = message.Timestamp,
                                Value = payload.Light.Value
                            });
                        }
                        if (payload.Smokedetector.HasValue)
                        {
                            processedData.Add(new ProcessedData
                            {
                                SensorId = "Smokedetektor",
                                Timestamp = message.Timestamp,
                                Value = payload.Smokedetector.Value
                            });
                        }

                    }
                }
                Console.WriteLine($"ВАЛИДАЦИЯ: УСПЕХ - создано {processedData.Count}");
                return processedData;
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 8,
                BoundedCapacity = 1000
            });

            // Блок обогащения данных
            //_enrichmentBlock = new TransformBlock<PayloadData, PayloadData>(data =>
           

            // Блок пакетной обработки(группируем по 100 сообщений)
            _batchingBlock = new BatchBlock<ProcessedData>(100);

            // Финальный блок для записи в базу
            _databaseWriterBlock = new ActionBlock<ProcessedData[]>(async batch =>
            {
                _logger.LogInformation($"Processed batch of {batch.Length} messages: {batch}");
                await WriteBatchToDatabase(batch.Where(x => x != null).ToArray());

            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 4// hm
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
            //// Здесь будет логика сохранения в базу данных, кэш и т.д.
            //// Пока просто логируем
            //Console.WriteLine("ОТПРАВИЛ В БАЗУ ХУЯЗУ");

            //// Имитация обработки
            //await Task.Delay(10);
            try
            {
                //преобразовываем данные в нужный вид
                var telemetrydata = batch
                    .Where(x => x.Value.HasValue).
                    Select(data => new SensorTelemetry
                    {
                        SensorId = data.SensorId,
                        Time = data.Timestamp,
                        Value = data.Value.Value

                    }).ToList();

                // Извлечение уникальных датчиков
                var uniqueSensors = batch
                    .Where(x => !string.IsNullOrEmpty(x.SensorId))
                    .GroupBy(x => x.SensorId)
                    .Select(g => new Sensor
                    {
                       SensorId = g.Key,
                    })
                    .ToList();




                // Создаем новый scope для этой операции
                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISensorTelemetryRepository>();
                var sensorrepository = scope.ServiceProvider.GetRequiredService<ISensorsRepository>();

                await repository.BatchInsertAsync(telemetrydata);
                await sensorrepository.BatchInsertAsync(uniqueSensors);
                _logger.LogInformation(
                 $"Записано {telemetrydata.Count} телеметрий, " +
                 $"обновлено {uniqueSensors.Count} датчиков");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data");
                throw;
            }
        }

        public async Task ProcessDataAsync(string data)
        {
            Console.WriteLine("ИДЕТ в конвейер");
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