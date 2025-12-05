using BenchmarkDotNet.Attributes;
using SmartHome.Infra.DataPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class PipelineBenchmark
{
    private ProcessingPipeline _pipeline;
    private List<SensorData> _testData;

    [GlobalSetup]
    public void Setup()
    {
        _pipeline = new ProcessingPipeline();
        _testData = GenerateTestData(10000);
    }

    [Benchmark]
    public async Task Process10000Messages()
    {
        var tasks = _testData.Select(data => _pipeline.ProcessDataAsync(data));
        await Task.WhenAll(tasks);
        await _pipeline.CompleteAsync();
    }

    private List<SensorData> GenerateTestData(int count)
    {
        var random = new Random();
        return Enumerable.Range(1, count)
        .Select(i => new SensorData
        {
            SensorId = $"sensor_{i % 1000}",
            Value = random.NextDouble() * 100,
            Timestamp = DateTime.UtcNow.AddSeconds(-random.Next(3600)),
            Quality = "Good"
        }).ToList();
    }
}