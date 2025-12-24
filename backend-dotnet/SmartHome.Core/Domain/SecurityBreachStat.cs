namespace SmartHome.Core.Domain;

public class SecurityBreachStat
{
    public DateTime Timestamp { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Resource { get; set; }
    public string? Details { get; set; }
    public SecuritySeverity Severity { get; set; }
}