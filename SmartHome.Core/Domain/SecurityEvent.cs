using SmartHomeAPI.Core.Entities;

namespace SmartHome.Core.Domain
{
    public class SecurityEvent
    {
        public DateTime Timestamp {get;set;}
        public User User {get;set;}
        public string Action {get;set;}
        public string Resource {get;set;}
        public string IpAddress {get;set;}
        public bool IsSuccessful {get;set;}
        public string Details {get;set;}
    }
}