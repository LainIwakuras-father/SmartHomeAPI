using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.Data;
using SmartHome.Infra.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartHome.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Требует аутентификации для всех методов
    public class TelemetryController : ControllerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IndustrialDbContext _context;

        private readonly ILogger _logger;
        private readonly ISensorTelemetryRepository _repository;
        private readonly ICache _cache;
        public TelemetryController(
            IndustrialDbContext context,
            ISensorTelemetryRepository repository,
            ILogger<TelemetryController>logger,
             IServiceScopeFactory serviceScopeFactory,
             ICache cache)
        {

            _context = context;
            _repository = repository;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _cache = cache;
        }

        // GET: api/TelemetryController
        [HttpGet("history")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<ActionResult<IEnumerable<SensorTelemetry>>> GetSensorTelemetry(
            [FromQuery] TelemetryHistoryRequest request)
        {
            // Валидация дат
            if (request.From > request.To)
            {
                return BadRequest("Дурак время перепутал");
            }
            try
            {
                var history = await _repository.GetHistoryTelemetry(
                    request.SensorId,
                    request.From,
                    request.To);

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving telemetry history");
                return StatusCode(500, "Internal server error");
            }
           
        }

        // Новый endpoint: получить последнее значение (быстро из кеша, fallback в БД)
        [HttpGet("{id}/latest")]
        [Authorize(Policy = "UserOrAdmin")]
        public async Task<ActionResult<SensorTelemetry>> GetLatest(string id)
        {
            try
            {
                var cached = await _cache.GetLatestAsync(id);
                if (cached != null)
                {
                    _logger.LogDebug("извлечение из кеша для датчика {SensorId}", id);
                    return Ok(cached);
                }

                // fallback на репозиторий: получим историю за весь период и возьмём первое (она возвращает в порядке убывания времени)
                var latestFromDb = await _repository.GetLatestSensorData(id);
                

                if (latestFromDb == null) return NotFound();

                return Ok(latestFromDb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest telemetry for {SensorId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}/data")]
        [Authorize(Policy = "AdminOnly")]
        public async Task GetSensorDataStream(string id)
        {
            // Server-Sent Events для потоковой передачи
            
                // Устанавливаем заголовки для SSE ОДИН РАЗ
                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");
                Response.Headers.Add("Access-Control-Allow-Origin", "*"); // Для CORS

                _logger.LogInformation($"Начало SSE потока для датчика: {id}");

                // Проверяем существование датчика
                using var scope = _serviceScopeFactory.CreateScope();
                var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorTelemetryRepository>();

                if (!await sensorRepo.SensorExists(id))
                {
                    await SendSseEventAsync("error", new { message = $"Датчик '{id}' не найден" });
                    return;
                }

                await SendSseEventAsync("connected", new { sensorId = id, timestamp = DateTime.UtcNow });

                var lastTime = DateTime.UtcNow.AddMinutes(-5); // Последние 5 минут

                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    try
                    {
                        // Получаем новые данные из БД
                        var repo = scope.ServiceProvider.GetRequiredService<ISensorTelemetryRepository>();
                        var newData = await repo.GetDataSinceAsync(id, lastTime);

                        foreach (var data in newData)
                        {
                            await SendSseEventAsync("data", data);
                            lastTime = data.Time > lastTime ? data.Time : lastTime;
                        }
                    }
                    catch (Exception ex)
                    {
                        await SendSseEventAsync("error", new { message = ex.Message });
                    }

                    await Task.Delay(2000); // Проверка каждые 2 секунды
                }

                await SendSseEventAsync("disconnected", new { timestamp = DateTime.UtcNow });
            
        }

        private async Task SendSseEventAsync(string eventName, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var message = $"event: {eventName}\ndata: {json}\n\n";
            await Response.WriteAsync(message);
            await Response.Body.FlushAsync();
        }

    }
}
