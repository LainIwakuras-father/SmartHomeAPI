namespace SmartHome.API.DTOs;
public class AuditStatsDto
{
    public int TotalEvents { get; set; }
    public int SuccessfulEvents { get; set; }
    public int FailedEvents { get; set; }
    public int RecentSecurityBreaches { get; set; }
    public int TodayEvents { get; set; }
    public Dictionary<string, int> ActionsDistribution { get; set; } = new();
    public List<ActiveUserDto> MostActiveUsers { get; set; } = new();
}