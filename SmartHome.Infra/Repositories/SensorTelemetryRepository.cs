using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.Data;

namespace SmartHome.Infra.Repositories
{
    public class SensorTelemetryRepository(IndustrialDbContext context) : ISensorTelemetryRepository, IDisposable
    {
        private bool _disposed;
        public async Task BatchInsertAsync(IEnumerable<SensorTelemetry> batch)
        {
            var records = batch as SensorTelemetry[] ?? [.. batch];
            if (!records.Any())
            {
                Console.WriteLine("Попытка сохранить пустую пачку данных");
            }

            await context.SensorTelemetry.AddRangeAsync(batch);

            await context.SaveChangesAsync();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    context?.Dispose();
                    
                }
                _disposed = true;
            }
        } 
        
        public async Task<IEnumerable<SensorTelemetry>> GetHistoryTelemetry(string? sensorId, DateTime from, DateTime to)
        {
            //// Автоматически исправляем если даты перепутаны
            //if (from > to)
            //{
            //    (from, to) = (to, from); // Меняем местами
            //}
            var query = context.SensorTelemetry
                .Where(t => t.Time >= from && t.Time <= to);
            if (!string.IsNullOrEmpty(sensorId))
            {
                query = query.Where(t => t.SensorId == sensorId);
            }
            return await query
                .OrderByDescending(t => t.Time)
                .ToListAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } 
       
    }
}
