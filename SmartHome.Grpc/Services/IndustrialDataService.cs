using Grpc.Core;
using SmartHome.Grpc;
using System.Threading.Channels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartHome.Grpc.Services
{
    public class IndustrialDataService
    {
        private readonly ILogger<IndustrialDataService> _logger;
        //private readonly DataProcessingPipeline _pipeline;
        //private readonly SensorCache _cache;
        public IndustrialDataService(ILogger<IndustrialDataService> logger)
        {
            _logger = logger;
            //_pipeline = pipeline;
            //_cache = cache;

        }

        public override async Task StreamSensorData(IAsyncStreamReader<SensorDataRequest> requestStream, IServerStreamWriter<SensorDataResponse> responseStream, ServerCallContext context)
        {
            var subscribedSensors = new HashSet<string>();

            // Читаем запросы от клиента
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                subscribedSensors.UnionWith(request.SensorIds);
            }
            // Создаем канал для передачи данных клиенту
            var channel = Channel.CreateUnbounded<SensorDataResponse>();

            // Симулируем потоковую передачу данных
            _ = Task.Run(async () =>
            {
                var random = new Random();
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    foreach (var sensorId in subscribedSensors)
                    {
                        var response = new SensorDataResponse
                        {
                            SensorId = sensorId,
                            Value = random.NextDouble() * 100,
                            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                        };
                        await channel.Writer.WriteAsync(response);
                    }
                    await Task.Delay(100, context.CancellationToken); // 10 обновлений в секунду
                }
                channel.Writer.Complete();
            });
        }
        // Отправляем данные клиенту
    //    await foreach (var data in channel.Reader.ReadAllAsync(context.CancellationToken))
    //     {
    //        await responseStream.WriteAsync(data);
    //}


}
}
