using Microsoft.EntityFrameworkCore;
using Prometheus;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;

using SmartHome.API;
using SmartHome.Infra.Data;
using SmartHome.Infra.DataPipeline;
using SmartHome.Infra.MessageBroker;
using SmartHome.Infra.Repositories;
using SmartHome.Infra.Settings;
using SmartHome.Infra.Security;
using SmartHome.Core.Interfaces;
using SmartHome.API.Controllers;

using SmartHome.Infra.Cache;
using SmartHome.Application.Service;


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
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
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

// Регистрация зависимостей 
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>(); 
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

builder.Services.AddSwaggerGen(c =>
{
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartHome", Version = "v001" });

                // Добавляем определение безопасности (Bearer Token)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Введите слово Bearer, пробел и ваш токен.\r\nПример: \"Bearer eyJhbGciOiJIUzI1NiIsInR5c...\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Добавляем требование безопасности ко всем методам
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
                // Кастомный фильтр для автоматической обработки токена
    // c.OperationFilter<SwaggerTokenOperationFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Конфигурация JWT аутентификации
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]?? 
    throw new InvalidOperationException("JWT Secret Key не настроен"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        // Важно: установите правильные типы claims
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
    // // Добавьте события для отладки
    // options.Events = new JwtBearerEvents
    // {
    //     OnAuthenticationFailed = context =>
    //     {
    //         Console.WriteLine($"OnAuthenticationFailed: {context.Exception.Message}");
    //         return Task.CompletedTask;
    //     },
    //     OnTokenValidated = context =>
    //     {
    //         Console.WriteLine($"OnTokenValidated - User: {context.Principal.Identity?.Name}");
    //         foreach (var claim in context.Principal.Claims)
    //         {
    //             Console.WriteLine($"  Claim: {claim.Type} = {claim.Value}");
    //         }
    //         return Task.CompletedTask;
    //     },
    //     OnChallenge = context =>
    //     {
    //         Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
    //         return Task.CompletedTask;
    //     }
    // };
});

builder.Services.AddAuthorization(options =>
{
    // Определение политик на основе ролей (RBAC)
    options.AddPolicy("UserOnly", policy => 
        policy.RequireRole("User"));
    options.AddPolicy("UserOrAdmin", policy => 
        policy.RequireRole("User", "Administrator"));
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Administrator"));
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









