namespace SmartHome.API.DTOs;

public class AuditEventDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Resource { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccessful { get; set; }
    public string? Details { get; set; }
}
