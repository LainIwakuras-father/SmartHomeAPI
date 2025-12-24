using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Entities
{
    public class SensorTelemetry
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string SensorId { get; set; }
        public double Value { get; set; }
    }
}
