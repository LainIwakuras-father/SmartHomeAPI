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
                User = auditEvent.User,
                Action = auditEvent.Action,
                Resource = auditEvent.Resource,
                IpAddress = auditEvent.IpAddress,
                IsSuccessful = auditEvent.IsSuccessful,
                Details = auditEvent.Details
            };

            await context.SecurityAudits.AddAsync(dbEvent);
            await context.SaveChangesAsync();
        }
    }
}

