using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SmartHome.Infra.Cache
{
    public class RedisCache : ICache
    {
        private readonly IConnectionMultiplexer _redis;
        //надо подумать какой объект туда запихнуть SensorTelemtry  Или ProcessedData
        private readonly Channel<SensorTelemetry> _updateChannel;
        //Базовые настройки 
        private readonly ILogger<RedisCache> _logger;
        public RedisCache(
            //IOptions<RedisOptions> options,
            ILogger<RedisCache> logger
           )
        {
            _logger = logger;
            //cоединяем с Redis
            //_redis = ConnectionMultiplexer.Connect(options)
            // Создаем канал для асинхронных обновлений
            _updateChannel = Channel.CreateBounded<SensorTelemetry>(1000);
            // Запускаем фоновую задачу для обновления кэша
            _ = Task.Run(ProcessCacheUpdates);

        }

        public async Task ProcessCacheUpdates()
        {
            throw new NotImplementedException();
            //await foreach (var data in _updateChannel.Reader.ReadAsync())
            //{


            //}
        }

        public void UpdateCache(SensorTelemetry data)
        {
            throw new NotImplementedException();
        }
    }
}
