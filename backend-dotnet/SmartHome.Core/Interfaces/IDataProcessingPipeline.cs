using SmartHome.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Interfaces
{
    public interface IDataProcessingPipeline
    {
        Task ProcessDataAsync(string data);
        //Task WriteBatchToDatabase(ProcessedData[] batch);
    }
}
