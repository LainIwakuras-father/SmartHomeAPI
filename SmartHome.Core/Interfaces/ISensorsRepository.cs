using SmartHome.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Interfaces
{
    public interface ISensorsRepository
    {
        Task<IEnumerable<Sensor>> GetSensors();
        Task BatchInsertAsync(IEnumerable<Sensor> sensors);
    }
}
