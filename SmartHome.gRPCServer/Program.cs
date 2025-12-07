using SmartHome.Core.Interfaces;
using SmartHome.gRPCServer.Services;
using SmartHome.Infra.Cache;
using SmartHome.Infra.DataPipeline;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16MB
    options.MaxSendMessageSize = 16 * 1024 * 1024; // 16MB
});
//builder.Services.AddSingleton<ProcessingPipeline>();
//builder.Services.AddSingleton<RedisCache>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<IndustrialDataService>();
app.MapGet("/", () => "Industrial Data Processing gRPC ServerCommunication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
