using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SmartHome.Core.Domain;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.DataPipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class PipelineBenchmark
{
    // инициализируем через null-forgiving — реальные значения устанавливаются в Setup
    private ProcessingPipeline _pipeline = null!;
    private List<string> _testData = null!;
    private List<long> _latenciesNs = new();
    [GlobalSetup]
    public void Setup()
    {
        // Простая тестовая реализация IServiceScopeFactory, которая возвращает scope с "толчковыми" репозиториями
        var scopeFactory = new TestServiceScopeFactory();

        // Null logger безопасен для бенчмарка
        var logger = NullLogger<ProcessingPipeline>.Instance;

        // Пустой (no-op) кеш, используется чтобы не падать при вызовах UpdateCache
        var cache = new DummyCache();

        // Создаём конвейер с "пустыми" зависимостями — записи в БД в тесте не выполняются реального I/O
        _pipeline = new ProcessingPipeline(logger, scopeFactory, cache);

        // Генерируем JSON-сообщения, которые парсер может распознать
        _testData = GenerateTestData(10000);



}

[Benchmark]
    public async Task Process10000Messages()
    {
        var tasks = _testData.Select(json => _pipeline.ProcessDataAsync(json));
        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Завершаем и ждём окончательной обработки
        await _pipeline.CompleteAsync().ConfigureAwait(false);
    }

    private string CreateTestMessageJson(int i)
    {
        var random = new Random(42);
        var now = DateTime.UtcNow;
        var payload = new
        {
            MessageId = Guid.NewGuid().ToString("N"),
            MessageType = "telemetry",
            PublisherId = $"sensor_{i % 1000}",
            Messages = new[]
                {
                    new {
                        Timestamp = now.AddSeconds(-random.Next(3600)),
                        Payload = new {
                            Humanidity = (double?)null,
                            Temperature = random.NextDouble() * 100,
                            Pressure = (double?)null,
                            Motion = (double?)null,
                            Light = (double?)null,
                            Smokedetector = (double?)null
                        }
                    }
                }
        };
        return JsonSerializer.Serialize(payload);
    }
    private List<string> GenerateTestData(int count)
    {
        var random = new Random(42);
        var now = DateTime.UtcNow;

        var list = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            // Сериализуем в JSON строку
            var json = CreateTestMessageJson(i);
            list.Add(json);
        }

        return list;
    }

    // --- вспомогательные тестовые типы (no-op реализации зависимостей) ---

    private class TestServiceScopeFactory : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => new TestScope();
    }

    private class TestScope : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; } = new TestServiceProvider();
        public void Dispose() { }
    }

    private class TestServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ISensorTelemetryRepository))
                return new TestSensorTelemetryRepository();
            if (serviceType == typeof(ISensorsRepository))
                return new TestSensorsRepository();

            return null;
        }
    }

    private class TestSensorTelemetryRepository : ISensorTelemetryRepository
    {
        public Task BatchInsertAsync(IEnumerable<SensorTelemetry> batch) => Task.CompletedTask;

        public Task<IEnumerable<SensorTelemetry>> GetHistoryTelemetry(string? sensorId, DateTime from, DateTime to)
            => Task.FromResult(Enumerable.Empty<SensorTelemetry>());

        public Task<bool> SensorExists(string sensorId) => Task.FromResult(true);

        public Task<IEnumerable<SensorTelemetry>> GetDataSinceAsync(string sensorId, DateTime sinceTime)
            => Task.FromResult(Enumerable.Empty<SensorTelemetry>());

        public Task<SensorTelemetry?> GetLatestSensorData(string sensorId)
            => Task.FromResult<SensorTelemetry?>(null);
    }

    private class TestSensorsRepository : ISensorsRepository
    {
        public Task BatchInsertAsync(IEnumerable<Sensor> batch) => Task.CompletedTask;

        public Task<IEnumerable<Sensor>> GetSensors()
            => Task.FromResult<IEnumerable<Sensor>>(Enumerable.Empty<Sensor>());
    }

    private class DummyCache : ICache
    {
        public Task ProcessCacheUpdates() => Task.CompletedTask;

        public void UpdateCache(SensorTelemetry data) { /* no-op */ }

        public Task<SensorTelemetry?> GetLatestAsync(string sensorId) => Task.FromResult<SensorTelemetry?>(null);
    }

    // Пример: внутри PipelineBenchmark (напр., в Setup или в самом бенчмарке)


    [Benchmark]
    public void Process10000Messages_RecordLatency()
    {
        for (int i = 0; i < 10_000; i++)
        {
            var sw = Stopwatch.StartNew();
            _pipeline.ProcessDataAsync(CreateTestMessageJson(i)).ConfigureAwait(false); // ваш реальный вызов
            sw.Stop();

            // перевод в наносекунды (точнее - через Stopwatch.Frequency)
            var ns = (long)(sw.ElapsedTicks * (1_000_000_000.0 / Stopwatch.Frequency));
            _latenciesNs.Add(ns);
        }
    }
    //private object CreateTestMessage(int i) => new { Id = i }; // заглушка
    // После прогона можно сохранить: File.WriteAllLines("latencies.csv", _latenciesNs.Select(x => x.ToString()));
}