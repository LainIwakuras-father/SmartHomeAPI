
using SmartHome.Core.Entities;

namespace SmartHomeAPI.Core.Entities
{
    public class User
    {
         public int Id { get; set; }
         public  string Username {get;set;}
         public  string Email {get;set;} 
         public  string HashPassword {get;set;}
         public  UserRole Role {get;set;} = UserRole.User;

        //ДОП ИНФОРМАЦИЯ
         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
         public DateTime? UpdatedAt { get; set; }

        // отношение одного ко многим
        public virtual ICollection<SecurityAuditRecord> SecurityAudits { get; set; } = [];

        public User() {}
         public User(string username, string email, UserRole role = UserRole.User)
         {
            
            Username = username;
            Email = email;
            Role = role;
            CreatedAt = DateTime.UtcNow;
         }
    }

   public enum UserRole
    {
        User = 1 ,   // пользователь
        Administrator = 2, // Администратор
        Auditor = 3        // Аудитор
    }
}