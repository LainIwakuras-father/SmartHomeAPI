using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartHome.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        // GET: api/status
        [HttpGet]
        public ActionResult<object> GetSystemStatus()
        {
            return Ok(new
            {
                Status = "Operational",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Uptime = Environment.TickCount / 1000
            });
        }
    }
}
