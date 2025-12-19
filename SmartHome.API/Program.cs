using Microsoft.EntityFrameworkCore;
using Prometheus;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using SmartHome.API;
using SmartHome.Infra.Data;
using SmartHome.Infra.DataPipeline;
using SmartHome.Infra.MessageBroker;
using SmartHome.Infra.Repositories;
using SmartHome.Infra.Settings;
using SmartHome.Core.Interfaces;
using SmartHome.API.Controllers;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using SmartHome.Infra.Cache;


Console.WriteLine("Starting..");
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddMetricServer(options =>
{
    options.Port = 9091;
    options.Hostname = "localhost";
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));
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

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<ISensorsRepository, SensorsRepository>();
builder.Services.AddScoped<ISensorTelemetryRepository, SensorTelemetryRepository>();
//добавляем кеш
// Добавление сервисов кэширования Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "SmartHome_"; // опционально, префикс для ключей
});
builder.Services.AddSingleton<ICache, RedisCache>();
builder.Services.AddSingleton<IDataProcessingPipeline, ProcessingPipeline>();
builder.Services.AddHostedService<MQTTSubcriberService>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
// Конфигурация JWT аутентификации
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // Определение политик на основе ролей (RBAC)
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));

    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("host", context => context.Request.Host.Host);
    options.InProgress.Enabled = true;
    options.RequestCount.Enabled = true;
    options.RequestDuration.Enabled = true;
});
app.MapMetrics("/metrics");



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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();









