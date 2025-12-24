using System.Security.Claims;

namespace SmartHome.Core.Interfaces
{
    public interface IJWTService
    {
        Task<string> GenerateJwtToken(string username, string role);
        ClaimsPrincipal ValidateToken(string token);
    }
}