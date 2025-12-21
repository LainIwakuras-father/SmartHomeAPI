using SmartHome.Core.Domain;

namespace SmartHome.Core.Interfaces
{
    public interface ISecurityAuditRepository
    {
        Task LogSecurityEventAsync(SecurityEvent auditEvent);
    }
}