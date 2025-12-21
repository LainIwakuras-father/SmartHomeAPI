using Microsoft.Extensions.Configuration;
using Npgsql;
using StackExchange.Redis;


namespace SmartHome.API
{
    public class TestScript
    {
        private readonly IConfiguration _configuration;

        // Добавляем конструктор для получения IConfiguration
        public TestScript(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // Тестовый скрипт для проверки подключений
        public async Task TestConnections()
        {

            // Проверка PostgreSQL
            try
            {
                using var connection = new NpgsqlConnection(
                _configuration.GetConnectionString("IndustrialDatabase"));
                await connection.OpenAsync();
                Console.WriteLine("PostgreSQL connection: OK");
                // Без Dapper - используем стандартный NpgsqlCommand
                using var command = new NpgsqlCommand("SELECT version();", connection);
                var version = await command.ExecuteScalarAsync();
                Console.WriteLine($"PostgreSQL version: {version}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostgreSQL connection failed: {ex.Message}");
            }
            // Проверка Redis
            try
            {
                var redis = ConnectionMultiplexer.Connect(
                _configuration.GetConnectionString("Redis"));
                var db = redis.GetDatabase();
                await db.PingAsync();
                Console.WriteLine("Redis connection: OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis connection failed: {ex.Message}");
            }

        }
    }
}