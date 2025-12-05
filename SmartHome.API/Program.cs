using Microsoft.EntityFrameworkCore;
using Prometheus;
using SmartHome.API;
using SmartHome.Infra.Data;
using SmartHome.Infra.DataPipeline;
using SmartHome.Infra.MessageBroker;
using SmartHome.Infra.Repositories;
using SmartHome.Infra.Settings;
using SmartHome.Core.Interfaces;
using SmartHome.API.Controllers;



Console.WriteLine("Starting..");
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

// ��������� ������ Prometheus
builder.Services.AddMetricServer(options =>
{
    options.Port = 9091;
    options.Hostname = "localhost";
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));
// ��������� Entity Framework Core
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
    // ������ ��� ����������
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
//����������� ����������� ���������� � �� ��� Scoped ��� ��� DbContex ���� scoped ����� � Singleton �� ����� ���� Scoped
builder.Services.AddScoped<ISensorsRepository, SensorsRepository>();
builder.Services.AddScoped<ISensorTelemetryRepository, SensorTelemetryRepository>();
// ������������ DI ProcessingPipeline
builder.Services.AddSingleton<IDataProcessingPipeline, ProcessingPipeline>();
//builder.Services.AddSingleton<MQTTSubcriber>();
//������������ ������� ������ ������� ������� MQTT
builder.Services.AddHostedService<MQTTSubcriberService>();
//����������� Npgsql ��� ������ � ��
// �����������
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// ��������� ����� ������ HTTP ��������
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("host", context => context.Request.Host.Host);
    options.InProgress.Enabled = true;
    options.RequestCount.Enabled = true;
    options.RequestDuration.Enabled = true;
});
// Endpoint ��� ������ Prometheus
app.MapMetrics("/metrics");


// ��������� MQTT ����������� ��� ������ ���������� ��� ������� ������ ������� �������� ��� � ���������� ������
//var mqttSubcriber = app.Services.GetRequiredService<MQTTSubcriber>();
// ��������� �������� ����������� ��� ������ ����������
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
// ✅ ВАЖНО: Добавьте правильный порядок middleware
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();





