using SmartHome.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Interfaces
{
    public interface IRealTimeDataStream
    {
        IAsyncEnumerable<SensorTelemetry> StreamSensorTelemetryAsync(IEnumerable<string> sensorIds, CancellationToken cancellationToken = default);
        event EventHandler<SensorTelemetry>? OnDataReceived;
    }
}
