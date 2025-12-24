using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Domain
{
    public class SensorMessage
    {
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string PublisherId { get; set; }
        public List<MessageData> Messages { get; set; }
    }

    public class MessageData
    {
        public DateTime Timestamp { get; set; }
        public PayloadData Payload { get; set; }
    }

    public class PayloadData
    {
        public double? Humanidity { get; set; }
        public double? Temperature { get; set; }
        public double? Pressure { get; set; }
        public double? Motion { get; set; }
        public double? Light { get; set; }
        public double? Smokedetector { get; set; }
    }
}
