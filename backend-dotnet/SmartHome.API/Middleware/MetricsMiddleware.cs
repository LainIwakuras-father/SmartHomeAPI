// Middleware/MetricsMiddleware.cs
using Prometheus;
using SmartHome.API.CustomMetrics;

namespace SmartHome.API.Middleware
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MetricsMiddleware> _logger;

        public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var endpoint = context.Request.Path;
            var method = context.Request.Method;

            try
            {
                await _next(context);
                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;

                // Записываем метрики API запроса
                SmartHomeMetrics.RecordApiRequest(endpoint, method, statusCode);
                SmartHomeMetrics.RecordProcessingDuration($"api_{method}", stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request to {Endpoint}", endpoint);
                throw;
            }
        }
    }
}