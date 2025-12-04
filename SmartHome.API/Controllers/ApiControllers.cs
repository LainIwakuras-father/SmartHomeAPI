//using Microsoft.AspNetCore.Mvc;
//using SmartHome.Core.Entities;
//using SmartHome.Infra.DataPipeline;


//namespace SmartHome.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class SensorsController : ControllerBase
//    {
//        private readonly ProcessingPipeline _pipeline;
//        //private readonly SensorCache _cache;
//        public SensorsController(
//            ProcessingPipeline pipeline)
//        {
//            _pipeline = pipeline;
//            //_cache = cache;

//        }
//        [HttpGet]
//        public IActionResult GetSensors([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
//        {
//            var sensors = Enumerable.Range(1, 1000)
//                .Select(i => new { Id = $"sensor_{i}", Type = "virtual", Status = "active" })
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize);

//            return Ok(new { Items = sensors, TotalCount = 1000 });
//        }
//        [HttpGet("{id}/data")]
//        public async IAsyncEnumerable<SensorTelemetry> GetSensorDataStream(string id)
//        {
//            // Server-Sent Events для потоковой передачи
//            Response.Headers.Add("Content-Type", "text/event-stream");
//            Response.Headers.Add("Cache-Control", "no-cache");
//            Response.Headers.Add("Connection", "keep-alive");

//            var random = new Random();
//            for (int i = 0; i < 100; i++) // Ограничим для демонстрации
//            {
//                if (HttpContext.RequestAborted.IsCancellationRequested)
//                    break;

//                var data = new SensorTelemetry
//                {
//                    SensorId = id,
//                    Value = random.NextDouble() * 100,
//                    Time = DateTime.UtcNow,
//                    //Quality = "Good"
//                };

//                yield return data;
//                await Task.Delay(1000); // 1 обновление в секунду
//            }
//        }
//        //SensorTelemetry[] или string[]
//        [HttpPost("batch")]
//        public async Task<IActionResult> ProcessBatch([FromBody] string[] batchData)
//        {
//            if (batchData == null || !batchData.Any())
//                return BadRequest("No data provided");

//            var processingTasks = batchData.Select(data =>
//           _pipeline.ProcessDataAsync(data));
//            await Task.WhenAll(processingTasks);

//            return Accepted(new
//            {
//                Message = "Batch processing started",
//                Count =
//           batchData.Length
//            });
//        }
//    }
//}

