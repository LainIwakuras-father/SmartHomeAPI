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

        public async Task<bool> SensorExists(string sensorId)
        {
            return await context.Sensors
                .AnyAsync(s => s.SensorId == sensorId);
        }

        public async Task<IEnumerable<SensorTelemetry>> GetDataSinceAsync(string sensorId, DateTime sinceTime)
        {
            
            return await context.SensorTelemetry
             .Where(x => x.SensorId == sensorId && x.Time > sinceTime)
             .OrderBy(x => x.Time)
             .Take(100)
             .AsNoTracking()
             .ToListAsync();

        }
        // Новая оптимизированная реализация: возвращает последнее значение для заданного датчика
        public async Task<SensorTelemetry?> GetLatestSensorData(string sensorId)
        {
            if (string.IsNullOrEmpty(sensorId)) return null;

            return await context.SensorTelemetry
                .Where(t => t.SensorId == sensorId)
                .OrderByDescending(t => t.Time)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
