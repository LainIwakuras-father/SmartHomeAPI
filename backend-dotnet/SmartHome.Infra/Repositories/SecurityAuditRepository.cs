using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Domain;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.Data;

namespace SmartHome.Infra.Repositories
{
    public class SecurityAuditRepository(IndustrialDbContext context): ISecurityAuditRepository
    {
        public async Task LogSecurityEventAsync(SecurityEvent auditEvent)
        {
            var dbEvent = new SecurityAuditRecord
            {
                Timestamp = DateTime.UtcNow,
                UserName = auditEvent.UserName,
                Action = auditEvent.Action,
                Resource = auditEvent.Resource,
                IpAddress = auditEvent.IpAddress,
                IsSuccessful = auditEvent.IsSuccessful,
                Details = auditEvent.Details
            };

            await context.SecurityAudits.AddAsync(dbEvent);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SecurityEvent>> GetRecentEventsAsync(int count = 100)
        {
            var records = await context.SecurityAudits
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync();

            return records.Select(r => new SecurityEvent
            {
                UserName = r.UserName,
                Action = r.Action,
                Resource = r.Resource,
                IpAddress = r.IpAddress,
                IsSuccessful = r.IsSuccessful,
                Details = r.Details,
                Timestamp = r.Timestamp
            });

            
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
            var query = context.SecurityAudits.AsQueryable();

            if (from.HasValue)
                query = query.Where(e => e.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.Timestamp <= to.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(e => e.UserName == userId);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(e => e.Action == action);

            if (isSuccessful.HasValue)
                query = query.Where(e => e.IsSuccessful == isSuccessful.Value);

            return await query
                .OrderByDescending(e => e.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await context.SecurityAudits.CountAsync();
        }
        public async Task<int> GetSuccessfulCountAsync()
        {
            return await context.SecurityAudits.CountAsync(e => e.IsSuccessful);
        }
        public async Task<int> GetSecurityBreachCountAsync(DateTime from)
        {
            return await context.SecurityAudits
                .CountAsync(e => e.Action == "SECURITY_BREACH" && e.Timestamp >= from);
        }

        public async Task<int> GetTodayCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await context.SecurityAudits
                .CountAsync(e => e.Timestamp >= today);
        }
        public async Task<IEnumerable<dynamic>> GetMostActiveUsersAsync(int top = 10)
        {
            return await context.SecurityAudits
                .Where(e => !string.IsNullOrEmpty(e.UserName))
                .GroupBy(e => e.UserName)
                .Select(g => new ActiveUserStat
                {
                    UserName = g.Key ?? "UNKNOWN",
                    EventCount = g.Count(),
                    LastActivity = g.Max(e => e.Timestamp),
                    SuccessfulActions = g.Count(e => e.IsSuccessful),
                    FailedActions = g.Count(e => !e.IsSuccessful)
                })
                .OrderByDescending(x => x.EventCount)
                .Take(top)
                .ToListAsync();
        }
        public async Task<IEnumerable<dynamic>> GetActionsDistributionAsync()
        {
            
            return await context.SecurityAudits
                .GroupBy(e => e.Action)
                .Select(g => new ActionDistribution
                {
                    Action = g.Key ?? "UNKNOWN",
                    Count = g.Count()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SecurityAuditRecord>> GetRecentSecurityBreachesAsync(int count = 10)
        {
            return await context.SecurityAudits
                .Where(e => e.Action == "SECURITY_BREACH")
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync();
        }
        public async Task<int> GetFailedLoginCountAsync(string username, DateTime from)
        {
            return await context.SecurityAudits
                .CountAsync(e => e.UserName == username
                    && e.Action == "LOGIN"
                    && !e.IsSuccessful
                    && e.Timestamp >= from);
        }

       
    }
}

