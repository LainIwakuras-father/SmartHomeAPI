namespace SmartHome.API.Settings
{
    public class DataProcessing
    {
        public int BatchSize { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public int BufferCapacity { get; set; }
        public int ProcessingTimeoutMs { get; set; }
    }
}
