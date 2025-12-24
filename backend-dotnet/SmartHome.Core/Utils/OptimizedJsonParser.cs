using SmartHome.Core.Domain;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartHome.Core.Utils
{   
    public  class OptimizedJsonParser
    { // Пул для байтовых массивов (переиспользование памяти)
        private static readonly ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;

        // Кэшированные JsonSerializerOptions для производительности
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultBufferSize = 1024,
        };
        // Статический метод для парсинга без аллокаций
        public static SensorMessage? ParseFromBytes(ReadOnlySpan<byte> jsonBytes)
        {
            try
            {
                // Используем Utf8JsonReader для парсинга без аллокаций строк
                var reader = new Utf8JsonReader(jsonBytes);

                var message = JsonSerializer.Deserialize<SensorMessage>(ref reader, _jsonOptions);
                if (message == null) return null;
                return message;
            }
            catch (JsonException)
            {
                return null;
            }
        }
        // Метод для парсинга из строки с оптимизацие
        public static SensorMessage ParseFromString(string jsonString)
        {
            // Преобразуем строку в байты один раз
            var byteCount = Encoding.UTF8.GetByteCount(jsonString);
            var byteArray = _bytePool.Rent(byteCount);
            try
            {
                var bytesWritten = Encoding.UTF8.GetBytes(jsonString, 0, jsonString.Length, byteArray, 0);
                return ParseFromBytes(new ReadOnlySpan<byte>(byteArray, 0, bytesWritten));
            }
            finally
            {
                _bytePool.Return(byteArray);
            }
        }
    }
}
