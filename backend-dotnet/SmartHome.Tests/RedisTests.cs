
//using System;
//using System.Linq;
//using System.Reflection;
//using System.Text.Json;
//using System.Threading.Tasks;
//using DotNet.Testcontainers.Builders;
//using DotNet.Testcontainers.Containers;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging.Abstractions;
//using SmartHome.Infra.Cache;
//using SmartHome.Infra.DataPipeline;
//using SmartHome.Core.Entities;
//using Xunit;

//public class ProcessingPipelineRedisIntegrationTests : IAsyncLifetime
//{
//    private readonly TestcontainersContainer _redisContainer;
//    private ServiceProvider _provider = null!;

//    public ProcessingPipelineRedisIntegrationTests()
//    {
//        _redisContainer = new TestcontainersBuilder<TestcontainersContainer>()
//            .WithImage("redis:7-alpine")
//            .WithName($"test-redis-{Guid.NewGuid():N}")
//            .WithCleanUp(true)
//            .WithPortBinding(6379, true)
//            .Build();
//    }

//    public async Task InitializeAsync()
//    {
//        await _redisContainer.StartAsync();

//        var redisHost = _redisContainer.Hostname;
//        var redisPort = _redisContainer.GetMappedPublicPort(6379);

//        var services = new ServiceCollection();
//        services.AddLogging();
//        // Add StackExchangeRedisCache pointing to container
//        services.AddStackExchangeRedisCache(options =>
//        {
//            options.Configuration = $"{redisHost}:{redisPort}";
//            options.InstanceName = "Test_";
//        });

//        // Register your RedisCache as ICache (uses IDistributedCache internally)
//        services.AddSingleton<ICache, RedisCache>();
//        services.AddSingleton<ProcessingPipeline>();
//        // IServiceScopeFactory available from BuildServiceProvider
//        _provider = services.BuildServiceProvider();
//    }

//    public async Task DisposeAsync()
//    {
//        if (_provider != null) await _provider.DisposeAsync();
//        await _redisContainer.StopAsync();
//    }

//    [Fact]
//    public async Task RedisCache_Writes_Keys_AreVisible_In_Redis()
//    {
//        // Arrange
//        var pipeline = _provider.GetRequiredService<ProcessingPipeline>();
//        var distCache = _provider.GetRequiredService<IDistributedCache>();
//        var serviceScopeFactory = _provider.GetRequiredService<IServiceScopeFactory>();
//        var logger = new NullLogger<ProcessingPipeline>();

//        // Создаём тестовые телеметрии
//        var telemetry = Enumerable.Range(1, 3)
//            .Select(i => new SensorTelemetry
//            {
//                SensorId = $"sensor{i}",
//                Time = DateTime.UtcNow,
//                Value = i * 1.5
//            })
//            .ToArray();

//        // Получаем приватный метод UpdateCacheUsingICacheAsync через reflection
//        var method = typeof(ProcessingPipeline).GetMethod("UpdateCacheUsingICacheAsync", BindingFlags.Instance | BindingFlags.NonPublic);
//        Assert.NotNull(method);

//        // Act: вызываем метод (через pipeline экземпляр)
//        var task = (Task)method.Invoke(pipeline, new object[] { telemetry })!;
//        await task.ConfigureAwait(false);

//        // Assert: проверяем, что ключи присутствуют в Redis
//        foreach (var t in telemetry)
//        {
//            var key = $"sensor:latest:{t.SensorId}";
//            var bytes = await distCache.GetAsync(key);
//            Assert.NotNull(bytes);

//            var doc = JsonDocument.Parse(bytes);
//            Assert.True(doc.RootElement.TryGetProperty("Value", out var val));
//            Assert.Equal(t.Value, val.GetDouble());
//        }
//    }
//}
