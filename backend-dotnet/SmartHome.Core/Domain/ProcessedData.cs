using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Domain
{
    //public enum DataQuality
    //{
    //    Good = 0,
    //    Warning = 1,
    //    Bad = 2
    //}

    public class ProcessedData
    {
        public string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public double? Value { get; set; }
        public override string ToString()
        {
            return $"SensorId: {SensorId}, Timestamp: {Timestamp}, Value: {Value}";
        }
    }
}
//public int? iValues { get; set; }
// public DataQuality Quality {get; set; }м