using SmartHome.Core.Domain;
using SmartHome.Core.Entities;

namespace SmartHome.Core.Interfaces
{
    public interface ISecurityAuditRepository
    {
        Task LogSecurityEventAsync(SecurityEvent auditEvent);

        Task<IEnumerable<SecurityEvent>> GetRecentEventsAsync(int count = 100);
        Task<IEnumerable<SecurityAuditRecord>> GetEventsAsync(
            DateTime? from = null,
            DateTime? to = null,
            string? userId = null,
            string? action = null,
            bool? isSuccessful = null,
            int skip = 0,
            int take = 100);

        

        Task<int> GetTotalCountAsync();
        Task<int> GetSuccessfulCountAsync();
        Task<int> GetSecurityBreachCountAsync(DateTime from);
        Task<int> GetTodayCountAsync();

        Task<IEnumerable<dynamic>> GetMostActiveUsersAsync(int top = 10);
        Task<IEnumerable<dynamic>> GetActionsDistributionAsync();
        Task<IEnumerable<SecurityAuditRecord>> GetRecentSecurityBreachesAsync(int count = 10);
        Task<int> GetFailedLoginCountAsync(string username, DateTime cutoffTime);
    }
}