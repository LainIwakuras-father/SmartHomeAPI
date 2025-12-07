using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SmartHome.Infra.Cache
{
    public class RedisCache : ICache, IDisposable
    {
        private readonly IDistributedCache _cache;
        private readonly Channel<SensorTelemetry> _updateChannel;
        private readonly ILogger<RedisCache> _logger;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _processingTask;

        public RedisCache(
            ILogger<RedisCache> logger,
            IDistributedCache cache
           )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            var options = new BoundedChannelOptions(1000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            };
            _updateChannel = Channel.CreateBounded<SensorTelemetry>(options);

            _processingTask = Task.Run(ProcessCacheUpdates, CancellationToken.None);
        }

        public void UpdateCache(SensorTelemetry data)
        {
            if (data == null) return;

            if (!_updateChannel.Writer.TryWrite(data))
            {
                _logger.LogWarning("Redis cache channel full, dropping update for sensor {SensorId}", data.SensorId);
            }
        }

        public async Task ProcessCacheUpdates()
        {
            try
            {
                var reader = _updateChannel.Reader;
                while (await reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    var batch = new System.Collections.Generic.List<SensorTelemetry>(16);
                    while (reader.TryRead(out var item))
                    {
                        batch.Add(item);
                        if (batch.Count >= 16) break;
                    }

                    if (batch.Count == 0) continue;

                    var tasks = new System.Collections.Generic.List<Task>(batch.Count);
                    foreach (var telemetry in batch)
                    {
                        try
                        {
                            var key = $"sensor:latest:{telemetry.SensorId}";
                            var payload = new
                            {
                                Value = telemetry.Value,
                                Time = telemetry.Time
                            };
                            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
                            var options = new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                            };
                            tasks.Add(_cache.SetAsync(key, bytes, options, _cts.Token));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to enqueue cache update for sensor {SensorId}", telemetry.SensorId);
                        }
                    }

                    try
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "One or more Redis cache writes failed in batch");
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in RedisCache.ProcessCacheUpdates");
            }
        }

        // Новая реализация чтения из кеша
        public async Task<SensorTelemetry?> GetLatestAsync(string sensorId)
        {
            if (string.IsNullOrEmpty(sensorId)) return null;

            var key = $"sensor:latest:{sensorId}";
            try
            {
                var bytes = await _cache.GetAsync(key, _cts.Token).ConfigureAwait(false);
                if (bytes == null || bytes.Length == 0) return null;

                using var doc = JsonDocument.Parse(bytes);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Value", out var valEl)) return null;
                if (!root.TryGetProperty("Time", out var timeEl)) return null;

                var value = valEl.GetDouble();
                var time = timeEl.GetDateTime();

                return new SensorTelemetry
                {
                    SensorId = sensorId,
                    Value = value,
                    Time = time
                };
            }
            catch (OperationCanceledException) { return null; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read latest from cache for sensor {SensorId}", sensorId);
                return null;
            }
        }

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                _updateChannel.Writer.Complete();
                _processingTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException) { }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}
