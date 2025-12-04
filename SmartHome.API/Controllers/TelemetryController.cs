using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.Data;

namespace SmartHome.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly IndustrialDbContext _context;

        private readonly ILogger _logger;
        private readonly ISensorTelemetryRepository _repository;

        public TelemetryController(
            IndustrialDbContext context,
            ISensorTelemetryRepository repository,
            ILogger<TelemetryController>logger)

        {
            _context = context;
            _repository = repository;
            _logger = logger;
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
        public async IAsyncEnumerable<SensorTelemetry> GetSensorDataStream(string id)
        {
            // Server-Sent Events для потоковой передачи
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            var random = new Random();
            for (int i = 0; i < 100; i++) // Ограничим для демонстрации
            {
                if (HttpContext.RequestAborted.IsCancellationRequested)
                    break;

                var data = new SensorTelemetry
                {
                    SensorId = id,
                    Value = random.NextDouble() * 100,
                    Time = DateTime.UtcNow,
                    //Quality = "Good"
                };

                yield return data;
                await Task.Delay(1000); // 1 обновление в секунду
            }
        }
        
    }
}
