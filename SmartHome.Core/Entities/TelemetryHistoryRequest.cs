using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Entities
{
    public class TelemetryHistoryRequest
    {
        public string? SensorId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
