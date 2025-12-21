using SmartHome.Core.Interfaces;


namespace SmartHome.Infra.Security
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        

        public bool VerifyPassword(string password, string hash) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password,hash);
    }
}