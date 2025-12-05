using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.Data;
using System.Threading.Tasks;

namespace SmartHome.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : ControllerBase
    {


        private readonly IndustrialDbContext _context;

        private readonly ILogger _logger;
        private readonly ISensorsRepository _repository;

        public SensorsController(
            IndustrialDbContext context,
            ISensorsRepository repository,
            ILogger<TelemetryController> logger)

        {
            _context = context;
            _repository = repository;
            _logger = logger;
        }

        // GET: api/Sensors
        [HttpGet]
        public async Task<IActionResult> GetSensors() 
        {
            var sensors = await _repository.GetSensors();
            return Ok(sensors);
        }
    }
}
