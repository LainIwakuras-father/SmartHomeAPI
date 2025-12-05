using SmartHome.Core.Domain;
using SmartHome.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Interfaces
{
    public interface ISensorTelemetryRepository
    {
        Task BatchInsertAsync(IEnumerable<SensorTelemetry> batch);
        Task<IEnumerable<SensorTelemetry>> GetHistoryTelemetry(string? sensorId, DateTime from, DateTime to);
       

        //для SSE для потока 
        Task<bool> SensorExists(string sensorId);
        //Task<SensorTelemetry> GetLatestSensorData(string sensorId);
        Task<IEnumerable<SensorTelemetry>> GetDataSinceAsync(string sensorId, DateTime sinceTime);

    }
}
