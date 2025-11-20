using Microsoft.AspNetCore.Mvc;
using SmartHome.API.DataPipeline;

namespace SmartHome.API.Controllers
{
    [ApiController]
    public class SensorsController: ControllerBase
    {
        private readonly ProcessingPipeline _pipeline;
        //private readonly SensorCache _cache;
        public SensorsController(
            ProcessingPipeline pipeline)
        {
            _pipeline = pipeline;
        }
        [HttpGet]
        public IActionResult GetSensors([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var sensors = Enumerable.Range(1,1000)
                .Select(i => new { Id = $"sensor_{i}", Type = "virtual", Status = "active" })
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return Ok(new { Items = sensors, TotalCount = 1000 });
        }
        [HttpGet("{id}/data")]
        public async IAsyncEnumerable<SensorData> GetSensorDataStream(string id)
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

                var data = new SensorData
                {
                    SensorId = id,
                    Value = random.NextDouble() * 100,
                    Timestamp = DateTime.UtcNow,
                    Quality = "Good"
                };

                yield return data;
                await Task.Delay(1000); // 1 обновление в секунду
            }
        }
        [HttpPost("batch")]
        public async Task<IActionResult> ProcessBatch([FromBody] SensorData[]batchData)
        {
            if (batchData == null || !batchData.Any())
                return BadRequest("No data provided");

            var processingTasks = batchData.Select(data =>
           _pipeline.ProcessDataAsync(data));
            await Task.WhenAll(processingTasks);

            return Accepted(new
            {
                Message = "Batch processing started",
                Count =
           batchData.Length
            });
        }
    }
}
}
