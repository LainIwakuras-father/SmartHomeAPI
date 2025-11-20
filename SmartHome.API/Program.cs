using Microsoft.EntityFrameworkCore;
using Prometheus;
using SmartHome.API;
using SmartHome.API.Data;
using SmartHome.API.DataPipeline;
using SmartHome.API.MessageBroker;
using SmartHome.API.Repositories;
using SmartHome.API.Settings;
using SmartHome.Core.Interfaces;



Console.WriteLine("Starting..");
var builder = WebApplication.CreateBuilder(args);

// Настройка метрик Prometheus
builder.Services.AddMetricServer(options =>
{
    options.Port = 9091;
    options.Hostname = "localhost";
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));
// Настройка Entity Framework Core
builder.Services.AddDbContext<IndustrialDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IndustrialDatabase"),

        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(300);
        });
    // Только для разработки
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
//builder.Services.ConfigureHttpJsonOptions(options =>
//{
//    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
//});
//Регистрация Репозитория работающий с БД как Scoped так как DbContex тоже scoped важно В Singleton не может быть Scoped
builder.Services.AddScoped<ISensorTelemetryRepository, SensorTelemetryRepository>();
// Регистрируем DI ProcessingPipeline
builder.Services.AddSingleton<IDataProcessingPipeline, ProcessingPipeline>();
//builder.Services.AddSingleton<MQTTSubcriber>();
//Регистрируем Фоновый Сервис Клиента Брокера MQTT
builder.Services.AddHostedService<MQTTSubcriberService>();
//Регистрация Npgsql для работы с БД
// Логирование
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

var app = builder.Build();

// Включение сбора метрик HTTP запросов
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("host", context => context.Request.Host.Host);
    options.InProgress.Enabled = true;
    options.RequestCount.Enabled = true;
    options.RequestDuration.Enabled = true;
});
// Endpoint для метрик Prometheus
app.MapMetrics("/metrics");


// Запускаем MQTT подключение при старте приложения как фоновый сервис который передает все в Обработчик данных
//var mqttSubcriber = app.Services.GetRequiredService<MQTTSubcriber>();
// Запускаем проверку подключений при старте приложения
try
{
    Console.WriteLine("Testing database and Redis connections...");

    var testScript = new TestScript(builder.Configuration);
    await testScript.TestConnections();

    Console.WriteLine("Connections test completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during connections test: {ex.Message}");
}

// Configure the HTTP request pipeline.
//app.MapGrpcService<GreeterService>();
//app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();





