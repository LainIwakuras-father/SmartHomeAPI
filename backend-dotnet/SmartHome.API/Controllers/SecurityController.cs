using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartHome.Core.Interfaces;
using SmartHome.API.DTOs;
using SmartHome.API.CustomMetrics;

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecurityAuditController : ControllerBase
    {
        private readonly ISecurityAuditService _auditService;

        public SecurityAuditController(ISecurityAuditService auditService)
        {
            _auditService = auditService;
        }
        
        [HttpGet("events")]
        [Authorize(Policy = "AdminOrAuditor")]
        public async Task<IActionResult> GetAuditEvents(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? user,
            [FromQuery] string? action,
            [FromQuery] bool? isSuccessful,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var events = await _auditService.GetEventsAsync(
                    from, to, user, action, isSuccessful, skip, take);


                SmartHomeMetrics.RecordMessageProcessed("audit_query", "success");
                SmartHomeMetrics.RecordProcessingDuration("audit_events", stopwatch.Elapsed.TotalSeconds);
                var eventDtos = events.Select(e => new AuditEventDto
                {
                    Id = e.Id,
                    Timestamp = e.Timestamp,
                    UserName = e.UserName,
                    Action = e.Action,
                    Resource = e.Resource,
                    IpAddress = e.IpAddress,
                    IsSuccessful = e.IsSuccessful,
                    Details = e.Details
                }).ToList();

                return Ok(eventDtos);
            }
            catch (Exception ex)
            {
                SmartHomeMetrics.RecordMessageProcessed("audit_query", "error");
                SmartHomeMetrics.RecordDatabaseError();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("failed-logins/{username}")]
        [Authorize(Policy = "AdminOrAuditor")]
        public async Task<IActionResult> GetFailedLoginCount(string username)
        {
            try
            {
                var count = await _auditService.GetFailedLoginCountAsync(username, TimeSpan.FromHours(24));

                return Ok(new
                {
                    Username = username,
                    FailedAttempts24h = count,
                    IsLocked = count >= 5
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("recent-security-breaches")]
        [Authorize(Policy = "AdminOrAuditor")]
        public async Task<IActionResult> GetRecentSecurityBreaches([FromQuery] int count = 10)
        {
            try
            {
                var breaches = await _auditService.GetRecentSecurityBreachesAsync(count);
                return Ok(breaches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("stats")]
        [Authorize(Policy = "AdminOrAuditor")]
        public async Task<IActionResult> GetAuditStats()
        {
            try
            {
                var stats = await _auditService.GetStatisticsAsync();
                var mostActiveUsers = await _auditService.GetMostActiveUsersAsync(10);

                var statsDto = new AuditStatsDto
                {
                    TotalEvents = stats.TotalEvents,
                    SuccessfulEvents = stats.SuccessfulEvents,
                    FailedEvents = stats.FailedEvents,
                    RecentSecurityBreaches = stats.RecentSecurityBreaches,
                    TodayEvents = stats.TodayEvents,
                    ActionsDistribution = stats.ActionsDistribution,
                    MostActiveUsers = mostActiveUsers.Select(u => new ActiveUserDto
                    {
                        UserName = u.UserName,
                        EventCount = u.EventCount,
                        LastActivity = u.LastActivity,
                        SuccessfulActions = u.SuccessfulActions,
                        FailedActions = u.FailedActions
                    }).ToList()
                };

                return Ok(statsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("summary")]
        [Authorize(Policy = "AdminOrAuditor")]
        public async Task<IActionResult> GetAuditSummary()
        {
            var stats = await _auditService.GetStatisticsAsync();
            var mostActiveUsers = await _auditService.GetMostActiveUsersAsync(5);
            var recentBreaches = await _auditService.GetRecentSecurityBreachesAsync(5);

            return Ok(new
            {
                Summary = new
                {
                    TotalAuditEvents = stats.TotalEvents,
                    SuccessRate = stats.TotalEvents > 0
                        ? Math.Round((double)stats.SuccessfulEvents / stats.TotalEvents * 100, 2)
                        : 0,
                    SecurityBreachesLast7Days = stats.RecentSecurityBreaches,
                    EventsToday = stats.TodayEvents
                },
                TopUsers = mostActiveUsers,
                RecentSecurityIncidents = recentBreaches,
                ActionsBreakdown = stats.ActionsDistribution
            });
        }
    }
}