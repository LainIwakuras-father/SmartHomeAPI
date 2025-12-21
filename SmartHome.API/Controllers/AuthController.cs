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

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        
        private readonly AuthService _authService;
        
        // В реальной системе здесь был бы сервис для проверки пользователей из БД
        public AuthController(
            
            AuthService authService)
        {
            
            _authService = authService;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            
           await _authService.Register(
            request.Username,
            request.Email,
            request.Password,
             request.Role);
           return NoContent();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
        // // ЗАГЛУШКА: Вместо этого должна быть проверка логина/пароля в базе данныx
        
        //     if (request.Username == "operator" && request.Password == "operator123")
        //     {
        //     var token = GenerateJwtToken(request.Username, "Operator");
        //     return Ok(new { Token = token });
        //     }
        //     else if (request.Username == "engineer" && request.Password == "engineer123")
        //     {
        //     var token = GenerateJwtToken(request.Username, "Engineer");
        //     return Ok(new { Token = token });
        //     }
        //     else if (request.Username == "admin" && request.Password == "admin123")
        //     {
        //     var token = GenerateJwtToken(request.Username, "Administrator");
        //     return Ok(new { Token = token });
        //     }
        //     return Unauthorized("Неверные учетные данные.");
            var token = await _authService.Login(request.Username,request.Password);
            return Ok(token);





        }
    
        
        // [HttpGet("validate")]
        // [Authorize]
        // public IActionResult Validate()
        // {
        //     var userName = User.Identity.Name;
        //     var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            
        //     return Ok(new
        //     {
        //         Message = "Токен действителен",
        //         User = userName,
        //         Role = role,
        //         ValidUntil = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
        //     });
        // }

            
    }

  
       
    
}