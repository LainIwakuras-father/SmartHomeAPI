using SmartHome.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddGrpc(options =>
//{
//    options.EnableDetailedErrors = true;
//    options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16MB
//    options.MaxSendMessageSize = 16 * 1024 * 1024; // 16MB
//    options.CompressionProviders = new List<ICompressionProvider>
//    {
//        new GzipCompressionProvider(System.IO.Compression.CompressionLevel.Fastest)
//    };
//    options.ResponseCompressionAlgorithm = "gzip";
//    options.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Fastest;
//});
//// Регистрация gRPC сервисов
//builder.Services.AddGrpcReflection();


//var app = builder.Build();

//// Configure the HTTP request pipeline.
//app.MapGrpcService<IndustrialDataService>();
//app.MapGrpcReflectionService();
//app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
//app.MapGet("/grpc/health", () => "gRPC service is healthy");
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16MB
    options.MaxSendMessageSize = 16 * 1024 * 1024; // 16MB
});
//builder.Services.AddSingleton<DataProcessingPipeline>();
//builder.Services.AddSingleton<SensorCache>();
app.MapGrpcService<IndustrialDataService>();
app.MapGet("/", () => "Industrial Data Processing gRPC Server");
app.Run();
