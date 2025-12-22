using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using SmartHome.Infra.Settings;
using SmartHome.Application.Service;
using System.Threading.Tasks;
using SmartHome.API.DTOs;
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

            try
            {
                await _authService.Register(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.Role);

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

            // var token = await _authService.Login(request.Username,request.Password);
            // return Ok(new { Token = token });
            try
            {
                
                if (await _auditService.HasExcessiveFailedAttemptsAsync(request.Username))
                {
                    await _auditService.LogSecurityBreachAsync(
                        "AuthController",
                        $"Excessive failed login attempts for user: {request.Username}",
                        SecuritySeverity.High);

                    return Unauthorized(new
                    {
                        Message = "Account temporarily locked due to multiple failed attempts"
                    });
                }

                var token = await _authService.Login(request.Username, request.Password);

                if (token != null)
                {
                   
                    await _auditService.LogLoginAttemptAsync(
                        request.Username,
                        true,
                        ipAddress,
                        "Login successful");

                    return Ok(new { Token = token });
                }
                else
                {
                    
                    await _auditService.LogLoginAttemptAsync(
                        request.Username,
                        false,
                        ipAddress,
                        "Invalid credentials");

                    return Unauthorized("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                
                await _auditService.LogLoginAttemptAsync(
                    request.Username,
                    false,
                    ipAddress,
                    $"Login error: {ex.Message}");

                _logger.LogError(ex, "Login failed for user: {Username}", request.Username);
                return Unauthorized("Login failed");
            }
        }
           
    }    
    
}