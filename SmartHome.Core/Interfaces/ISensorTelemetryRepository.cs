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

    }
}
