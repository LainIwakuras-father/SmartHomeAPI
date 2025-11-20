using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        //        rivate static readonly Counter MessagesProcessed = Metrics
        // .CreateCounter("messages_processed_total", "Total number of processed
        //messages");

        // private static readonly Gauge ProcessingDuration = Metrics
        // .CreateGauge("processing_duration_seconds", "Data processing duration")

        //        private static readonly Histogram MessageLatency = Metrics
        // .CreateHistogram("message_latency_seconds", "Message processing
        //latency");

        [HttpGet]
        public IActionResult GetMetrics()
        {
            return Ok();
        }
        public static void RecordMessageProcessed(double processingTime)
        {
            MessagesProcessed.Inc();
            ProcessingDuration.Set(processingTime);
            MessageLatency.Observe(processingTime);
        }
    }
}

