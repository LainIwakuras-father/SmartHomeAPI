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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
