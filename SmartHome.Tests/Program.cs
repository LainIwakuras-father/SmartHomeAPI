using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;

using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Loggers; // для PlotExporter

var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddDiagnoser(MemoryDiagnoser.Default)      // замеры аллокаций/Gen0..Gen2
    .AddExporter(HtmlExporter.Default)          // HTML отчёт (таблицы)
    .AddExporter(CsvExporter.Default)           // таблицы для построения графиков
    .AddExporter(JsonExporter.Default);       // машинно-читаемый результат        // PNG/SVG графики

BenchmarkRunner.Run<PipelineBenchmark>(config);
BenchmarkDotNet.Loggers.ConsoleLogger.Default.WriteLine("Benchmarking completed."); 





