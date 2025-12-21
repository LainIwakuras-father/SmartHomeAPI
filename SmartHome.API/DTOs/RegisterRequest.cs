using SmartHomeAPI.Core.Entities;

namespace SmartHome.API.DTOs;
 public class RegisterRequest
{
            public string Username { get; set; }
            public string  Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public UserRole Role { get; set; } = UserRole.User;
}