using SmartHome.Core.Entities;
using System.Threading.Tasks;

namespace SmartHome.Core.Interfaces
{
    public interface ICache
    {
        Task ProcessCacheUpdates();
        void UpdateCache(SensorTelemetry data);

        // Новая функция чтения последнего значения из кеша
        Task<SensorTelemetry?> GetLatestAsync(string sensorId);
    }
}
