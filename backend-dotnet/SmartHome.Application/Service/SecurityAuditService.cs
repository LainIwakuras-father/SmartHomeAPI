using Microsoft.Extensions.Logging;
using SmartHome.Core.Domain;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;

namespace SmartHome.Application.Service;

public class SecurityAuditService(
    ILogger<SecurityAuditService> _logger, ISecurityAuditRepository _securityAuditRepository
) : ISecurityAuditService
{
    
    public async Task LogEventAsync(SecurityEvent auditEvent)
    {
        try
        {
            await _securityAuditRepository.LogSecurityEventAsync(auditEvent);
            var logLevel = auditEvent.IsSuccessful ? LogLevel.Information : LogLevel.Warning;
            _logger.Log(logLevel,
            "🔒 Security Event: Action={Action}, User={User}, Success={Success}, Resource={Resource}",
                auditEvent.Action, auditEvent.UserName, auditEvent.IsSuccessful, auditEvent.Resource);
        }catch(Exception e)
        {
            _logger.LogError(e ,"Ошибка Автоматического Аудитора Безопасности");
        }
    }


    public async Task<bool> HasExcessiveFailedAttemptsAsync(string username)
    {
        
        var failedCount = await GetFailedLoginCountAsync(username, TimeSpan.FromMinutes(15));
        return failedCount >= 5;
    }

    public Task LogDataAccessAsync(string username, string resource, bool isSuccessful, string? details = null)
    {
        var auditEvent = SecurityEvent.CreateDataAccessEvent(username, resource, isSuccessful, details);
        return LogEventAsync(auditEvent);
    }



    public Task LogLoginAttemptAsync(string username, bool isSuccessful, string? ipAddress = null, string? details = null)
    {
        var auditEvent = SecurityEvent.CreateLoginEvent(username, isSuccessful, ipAddress, details);
        return LogEventAsync(auditEvent);
    }
    public Task LogSecurityBreachAsync(string resource, string details, SecuritySeverity severity)
    {
        var auditEvent = SecurityEvent.CreateSecurityBreachEvent(resource, details, severity);
        return LogEventAsync(auditEvent);
    }
    public async Task<IEnumerable<SecurityAuditRecord>> GetEventsAsync(
            DateTime? from = null,
            DateTime? to = null,
            string? userId = null,
            string? action = null,
            bool? isSuccessful = null,
            int skip = 0,
            int take = 100)
    {
        return await _securityAuditRepository.GetEventsAsync(
            from, to, userId, action, isSuccessful, skip, take);
    }


    // Новые методы для статистики
    public async Task<AuditStatistics> GetStatisticsAsync()
    {
        var totalEvents = await _securityAuditRepository.GetTotalCountAsync();
        var successfulEvents = await _securityAuditRepository.GetSuccessfulCountAsync();
        var recentBreaches = await _securityAuditRepository.GetSecurityBreachCountAsync(DateTime.UtcNow.AddDays(-7));
        var todayEvents = await _securityAuditRepository.GetTodayCountAsync();
        var actionsDistribution = await _securityAuditRepository.GetActionsDistributionAsync();

        var stats = new AuditStatistics
        {
            TotalEvents = totalEvents,
            SuccessfulEvents = successfulEvents,
            FailedEvents = totalEvents - successfulEvents,
            RecentSecurityBreaches = recentBreaches,
            TodayEvents = todayEvents,
            ActionsDistribution = new Dictionary<string, int>()
        };

        foreach (var action in actionsDistribution)
        {
            stats.ActionsDistribution[action.Action] = action.Count;
        }

        return stats;
    }

    public async Task<int> GetFailedLoginCountAsync(string username, TimeSpan timeWindow)
    {
        var cutoffTime = DateTime.UtcNow - timeWindow;
        return await _securityAuditRepository.GetFailedLoginCountAsync(username, cutoffTime);
    }

    public async Task<IEnumerable<ActiveUserStat>> GetMostActiveUsersAsync(int top = 10)
    {
        var users = await _securityAuditRepository.GetMostActiveUsersAsync(top);

        return users.Select(u => new ActiveUserStat
        {
            UserName = u.UserName,
            EventCount = u.EventCount,
            LastActivity = u.LastActivity,
            SuccessfulActions = u.SuccessfulActions,
            FailedActions = u.FailedActions
        });
    }

    public async Task<IEnumerable<SecurityBreachStat>> GetRecentSecurityBreachesAsync(int count = 10)
    {
        var breaches = await _securityAuditRepository.GetRecentSecurityBreachesAsync(count);

        return breaches.Select(b => new SecurityBreachStat
        {
            Timestamp = b.Timestamp,
            UserName = b.UserName,
            Action = b.Action,
            Resource = b.Resource,
            Details = b.Details,
            Severity = SecuritySeverity.High // Можно добавить поле в сущность
        });
    }
}