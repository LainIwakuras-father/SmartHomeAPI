// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using SmartHome.Infra.Data;

// namespace SmartHome.Infra.Services
// {
//     public class AuditCleanupService : BackgroundService
//     {
//         private readonly ILogger<AuditCleanupService> _logger;
//         private readonly IServiceScopeFactory _scopeFactory;

//         public AuditCleanupService(ILogger<AuditCleanupService> logger, IServiceScopeFactory scopeFactory)
//         {
//             _logger = logger;
//             _scopeFactory = scopeFactory;
//         }

//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     using var scope = _scopeFactory.CreateScope();
//                     var context = scope.ServiceProvider.GetRequiredService<IndustrialDbContext>();

//                     // Удаляем записи старше 90 дней
//                     var cutoffDate = DateTime.UtcNow.AddDays(-90);
//                     var oldRecords = await context.SecurityAudits
//                         .Where(e => e.Timestamp < cutoffDate)
//                         .ToListAsync(stoppingToken);

//                     if (oldRecords.Any())
//                     {
//                         context.SecurityAudits.RemoveRange(oldRecords);
//                         await context.SaveChangesAsync(stoppingToken);

//                         _logger.LogInformation("Cleaned up {Count} old audit records", oldRecords.Count);
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error during audit cleanup");
//                 }

//                 // Запускаем очистку раз в день
//                 await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
//             }
//         }
//     }
// }