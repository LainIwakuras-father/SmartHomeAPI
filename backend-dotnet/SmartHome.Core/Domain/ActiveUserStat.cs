namespace SmartHome.Core.Domain;

public class ActiveUserStat
{
    public string? UserName { get; set; }
    public int EventCount { get; set; }
    public DateTime LastActivity { get; set; }
    public int SuccessfulActions { get; set; }
    public int FailedActions { get; set; }
}