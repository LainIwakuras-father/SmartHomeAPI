using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        // В реальной системе здесь был бы сервис для проверки пользователей из БД
        public AuthController(IOptions<JwtSettings> jwtSettings)
        {
        _jwtSettings = jwtSettings.Value;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
        // ЗАГЛУШКА: Вместо этого должна быть проверка логина/пароля в базе данны
        
            if (request.Username == "operator" && request.Password == "operator123")
            {
            var token = GenerateJwtToken(request.Username, "Operator");
            return Ok(new { Token = token });
            }
            else if (request.Username == "engineer" && request.Password == "engineer123")
            {
            var token = GenerateJwtToken(request.Username, "Engineer");
            return Ok(new { Token = token });
            }
            else if (request.Username == "admin" && request.Password == "admin123")
            {
            var token = GenerateJwtToken(request.Username, "Administrator");
            return Ok(new { Token = token });
            }
            return Unauthorized("Неверные учетные данные.");
        }
    
        private string GenerateJwtToken(string username, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }),

            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
            


        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        
    }
}