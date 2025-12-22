// using SmartHomeAPI.Core.Entities;

// namespace SmartHome.Core.Domain
// {
//     public class SecurityEvent
//     {
//         public DateTime Timestamp {get;set;}
//         public User User {get;set;}
//         public string Action {get;set;}
//         public string Resource {get;set;}
//         public string IpAddress {get;set;}
//         public bool IsSuccessful {get;set;}
//         public string Details {get;set;}
//     }
// }


namespace SmartHome.Core.Domain
{
    public enum SecurityActionType
    {
        Login,
        Logout,
        DataRead,
        DataWrite,
        ConfigurationChange,
        SecurityBreach,
        AccessDenied,
        UserCreated,
        UserModified,
        UserDeleted
    }

    public enum SecuritySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class SecurityEvent
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public SecurityActionType ActionType { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Resource { get; set; }
        public string? IpAddress { get; set; }
        public bool IsSuccessful { get; set; }
        public SecuritySeverity Severity { get; set; }
        public string? Details { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static SecurityEvent CreateLoginEvent(string username, bool isSuccessful, string? ip = null, string? details = null)
        {
            return new SecurityEvent
            {
                UserName = username,
                Action = "LOGIN",
                ActionType = SecurityActionType.Login,
                IpAddress = ip,
                IsSuccessful = isSuccessful,
                Severity = isSuccessful ? SecuritySeverity.Low : SecuritySeverity.High,
                Details = details,
                Resource = "AuthController"
            };
        }

        public static SecurityEvent CreateDataAccessEvent(string username, string resource, bool isSuccessful, string? details = null)
        {
            return new SecurityEvent
            {
                UserName = username,
                Action = "DATA_ACCESS",
                ActionType = SecurityActionType.DataRead,
                Resource = resource,
                IsSuccessful = isSuccessful,
                Severity = isSuccessful ? SecuritySeverity.Low : SecuritySeverity.Medium,
                Details = details
            };
        }

        public static SecurityEvent CreateSecurityBreachEvent(string resource, string details, SecuritySeverity severity)
        {
            return new SecurityEvent
            {
                Action = "SECURITY_BREACH",
                ActionType = SecurityActionType.SecurityBreach,
                Resource = resource,
                IsSuccessful = false,
                Severity = severity,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}