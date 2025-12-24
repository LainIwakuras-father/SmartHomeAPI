using Prometheus;

namespace SmartHome.API.CustomMetrics
{
    public static class SmartHomeMetrics
    {
        // Метрики обработки сообщений
        public static readonly Counter MessagesProcessed = Metrics
            .CreateCounter("smart_home_messages_processed_total",
                "Total number of processed smart home messages",
                new CounterConfiguration
                {
                    LabelNames = new[] { "sensor_type", "status" }
                });

        public static readonly Histogram ProcessingDuration = Metrics
            .CreateHistogram("smart_home_processing_duration_seconds",
                "Histogram of smart home data processing durations",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(0.001, 2, 16),
                    LabelNames = new[] { "operation_type" }
                });

        public static readonly Gauge ActiveConnections = Metrics
            .CreateGauge("smart_home_active_connections",
                "Number of active connections to smart home services");

        public static readonly Counter DatabaseErrors = Metrics
            .CreateCounter("smart_home_database_errors_total",
                "Total number of database errors in smart home system");

        // Метрики безопасности (добавляем к существующим)
        public static readonly Counter AuthenticationFailures = Metrics
            .CreateCounter("smart_home_auth_failures_total",
                "Number of failed authentication attempts",
                new CounterConfiguration
                {
                    LabelNames = new[] { "username", "reason" }
                });

        public static readonly Counter AuthenticationSuccesses = Metrics
            .CreateCounter("smart_home_auth_successes_total",
                "Number of successful authentication attempts",
                new CounterConfiguration
                {
                    LabelNames = new[] { "username", "role" }
                });

        public static readonly Counter JwtTokensIssued = Metrics
            .CreateCounter("smart_home_jwt_tokens_issued_total",
                "Number of JWT tokens issued");

        public static readonly Gauge ActiveUsers = Metrics
            .CreateGauge("smart_home_active_users",
                "Number of active authenticated users");

        // Метрики API запросов
        public static readonly Counter ApiRequests = Metrics
            .CreateCounter("smart_home_api_requests_total",
                "Total number of API requests",
                new CounterConfiguration
                {
                    LabelNames = new[] { "endpoint", "method", "status_code" }
                });

        // Вспомогательные методы для удобства
        public static void RecordMessageProcessed(string sensorType, string status)
        {
            MessagesProcessed.WithLabels(sensorType, status).Inc();
        }

        public static void RecordProcessingDuration(string operationType, double durationSeconds)
        {
            ProcessingDuration.WithLabels(operationType).Observe(durationSeconds);
        }

        public static void RecordDatabaseError()
        {
            DatabaseErrors.Inc();
        }

        public static void RecordAuthSuccess(string username, string role)
        {
            AuthenticationSuccesses.WithLabels(username, role).Inc();
            ActiveUsers.Inc();
        }

        public static void RecordAuthFailure(string username, string reason)
        {
            AuthenticationFailures.WithLabels(username, reason).Inc();
        }

        public static void RecordJwtTokenIssued()
        {
            JwtTokensIssued.Inc();
        }

        public static void RecordApiRequest(string endpoint, string method, int statusCode)
        {
            ApiRequests.WithLabels(endpoint, method, statusCode.ToString()).Inc();
        }
    }
}