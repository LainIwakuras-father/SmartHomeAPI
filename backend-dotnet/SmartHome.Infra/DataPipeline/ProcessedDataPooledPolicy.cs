using Microsoft.Extensions.ObjectPool;
using SmartHome.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Infra.DataPipeline
{
    // Класс для работы с пулом объектов ProcessedData
    public class ProcessedDataPooledPolicy : IPooledObjectPolicy<ProcessedData>
    {
        public ProcessedData Create() => new ProcessedData();
       

        public bool Return(ProcessedData obj)
        {
            if (obj == null) return false;
        
            // Сбрасываем значения для повторного использования
            obj.SensorId = null;
            obj.Timestamp = default;
            obj.Value = null;
            return true;
        }
    }
}
