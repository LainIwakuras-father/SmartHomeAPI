using SmartHomeAPI.Core.Entities;

namespace SmartHome.Core.Entities
{
    public record SecurityAuditRecord
    {
        public int Id {get;set;}
        public DateTime Timestamp {get;set;}
        // Внешний ключ на User
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        
        public string Action {get;set;} = string.Empty;
        public string? Resource {get;set;}
        public string? IpAddress {get;set;}
        public bool IsSuccessful {get;set;}
        public string? Details {get;set;}
        //отношение одного ко многим
        public virtual User? User {get;set;}
    }
}