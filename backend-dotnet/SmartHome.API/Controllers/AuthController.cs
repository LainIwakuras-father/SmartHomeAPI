using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartHome.Application.Service;

using SmartHome.API.DTOs;
using SmartHome.API.CustomMetrics;
using SmartHome.Core.Interfaces;
using SmartHome.Core.Domain;

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        
        private readonly AuthService _authService;
        private readonly ISecurityAuditService _auditService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(

            AuthService authService,
            ISecurityAuditService auditService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await _authService.Register(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.Role);

                // Записываем метрику успешной регистрации
                SmartHomeMetrics.RecordMessageProcessed("user_registration", "success");
                SmartHomeMetrics.RecordProcessingDuration("registration", stopwatch.Elapsed.TotalSeconds);

                // Логируем создание пользователя
                await _auditService.LogEventAsync(new SecurityEvent
                {
                    UserName = "System",
                    Action = "USER_CREATED",
                    Resource = request.Username,
                    IpAddress = ipAddress,
                    IsSuccessful = true,
                    Details = $"New user registered: {request.Username}",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                // Записываем метрику неудачной регистрации
                SmartHomeMetrics.RecordMessageProcessed("user_registration", "failure");
                SmartHomeMetrics.RecordDatabaseError();
                await _auditService.LogEventAsync(new SecurityEvent
                {
                    UserName = "System",
                    Action = "USER_CREATION_FAILED",
                    Resource = request.Username,
                    IpAddress = ipAddress,
                    IsSuccessful = false,
                    Details = $"Registration failed: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                });

                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                
                if (await _auditService.HasExcessiveFailedAttemptsAsync(request.Username))
                {
                    SmartHomeMetrics.RecordAuthFailure(request.Username, "rate_limit");
                    await _auditService.LogSecurityBreachAsync(
                        "AuthController",
                        $"Неудачная попытка входа user: {request.Username}",
                        SecuritySeverity.High);

                    return Unauthorized(new
                    {
                        Message = "Слишком много неудачных попыток входа"
                    });
                }

                var token = await _authService.Login(request.Username, request.Password);

                if (token != null)
                {
                    // Записываем метрики успешного входа
                    // SmartHomeMetrics.RecordAuthSuccess(request.Username, userRole);
                    SmartHomeMetrics.RecordJwtTokenIssued();
                    SmartHomeMetrics.RecordProcessingDuration("login", stopwatch.Elapsed.TotalSeconds);
                    await _auditService.LogLoginAttemptAsync(
                        request.Username,
                        true,
                        ipAddress,
                        "Login successful");

                    return Ok(new { Token = token });
                }
                else
                {
                    SmartHomeMetrics.RecordAuthFailure(request.Username, "invalid_credentials");
                    await _auditService.LogLoginAttemptAsync(
                        request.Username,
                        false,
                        ipAddress,
                        "Неверные Логин или пароль");

                    return Unauthorized("Неверные Логин или пароль");
                }
            }
            catch (Exception ex)
            {
                SmartHomeMetrics.RecordAuthFailure(request.Username, "exception");
                await _auditService.LogLoginAttemptAsync(
                    request.Username,
                    false,
                    ipAddress,
                    $"Login error: {ex.Message}");

                _logger.LogError(ex, "Login Ошибка для user: {Username}", request.Username);
                return Unauthorized("Login Ошибка");
            }
        }
           
    }    
    
}