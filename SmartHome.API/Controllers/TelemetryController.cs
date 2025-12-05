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
    public class TelemetryController : ControllerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IndustrialDbContext _context;

        private readonly ILogger _logger;
        private readonly ISensorTelemetryRepository _repository;

        public TelemetryController(
            IndustrialDbContext context,
            ISensorTelemetryRepository repository,
            ILogger<TelemetryController>logger,
             IServiceScopeFactory serviceScopeFactory)
        {

            _context = context;
            _repository = repository;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        // GET: api/TelemetryController
        [HttpGet("history")]
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
        [HttpGet("{id}/data")]
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
