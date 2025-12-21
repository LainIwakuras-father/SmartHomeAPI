using SmartHome.Infra.Repositories;
using SmartHome.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SmartHome.Core.Entities;
using SmartHomeAPI.Core.Entities;
using StackExchange.Redis;

namespace SmartHome.Application.Service
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJWTService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJWTService jWTService,
            ILogger<AuthService> logger
        )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jWTService;
            _logger = logger;
        }


        public async Task Register(
            string username, 
            string email, 
            string password, 
            UserRole role = UserRole.User

        )
        {
            // //1. хешировать пароль
            // 2. Создать объект юзера
            //3. добавить в БД
            //             // await _userRepository.AddAsync(user);
            var hash = _passwordHasher.HashPassword(password);
            var user = new User
            {
                Username = username,
                Email = email,
                HashPassword = hash,
                Role = role
            };
            await _userRepository.AddAsync(user);
             _logger.LogInformation($"Пользователь {username} успешно зарегистрирован");
        }

        public async Task<string> Login(string username,string password)
        {
            //var veryfyPASS
            //1. проверить пароль
            //2.Проверить существует ли пользователь
            //3.создать токен доступа

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
              _logger.LogWarning($"Попытка входа для несуществующего пользователя {username}");
               return null;
            }
            var verifygood = _passwordHasher.VerifyPassword(password,user.HashPassword);
            if (!verifygood)
            {
                _logger.LogWarning($"Неудачная попытка входа для пользователя {username}");
            }
            var role = user.Role.ToString();
            //genereteJWT
            return await _jwtService.GenerateJwtToken(user.Username,role);
        }
    }
}