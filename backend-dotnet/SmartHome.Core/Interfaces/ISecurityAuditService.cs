using SmartHome.Core.Domain;
using SmartHome.Core.Entities;

namespace SmartHome.Core.Interfaces
{
    public interface ISecurityAuditService
    {
        Task LogEventAsync(SecurityEvent auditEvent);
        Task LogLoginAttemptAsync(string username, bool isSuccessful, string? ipAddress = null, string? details = null);
        Task LogDataAccessAsync(string username, string resource, bool isSuccessful, string? details = null);
        Task LogSecurityBreachAsync(string resource, string details, SecuritySeverity severity);
        Task<IEnumerable<SecurityAuditRecord>> GetEventsAsync(
            DateTime? from = null,
            DateTime? to = null,
            string? userId = null,
            string? action = null,
            bool? isSuccessful = null,
            int skip = 0,
            int take = 100);

        Task<int> GetFailedLoginCountAsync(string username, TimeSpan timeWindow);
        Task<bool> HasExcessiveFailedAttemptsAsync(string username);

        // Новые методы для статистики
        Task<AuditStatistics> GetStatisticsAsync();
        Task<IEnumerable<ActiveUserStat>> GetMostActiveUsersAsync(int top = 10);
        Task<IEnumerable<SecurityBreachStat>> GetRecentSecurityBreachesAsync(int count = 10);
    }
}