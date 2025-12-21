using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Entities
{
 //   message StatisticsResponse
 //   {
 //double average = 1;
 //double min = 2;
 //double max = 3;
 //double standard_deviation = 4;
 //   }
    public class StaticticsResponse
    {
        public double Average { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double standard_deviation { get; set; }
    }
}
