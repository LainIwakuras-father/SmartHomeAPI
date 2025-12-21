using SmartHome.Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Formatter;
using SmartHome.Core.Interfaces;
using System.Text;
using System.Threading.Tasks;
using SmartHome.Infra.Settings;
using Microsoft.Extensions.Hosting;

namespace SmartHome.Infra.MessageBroker
{
    // в этот клиент нужно внедрить перемещения сообщения в TPL конвейер
    public class MQTTSubcriberService : BackgroundService
    {
        //конвейер обработки данных
        private readonly IDataProcessingPipeline _pipeline;
        private readonly MqttSettings _settings;
        private readonly ILogger<MQTTSubcriberService> _logger;
        //создаем клиенты
        private IMqttClient _mqttClient;
        // добавляем зависимости через конструктор
        public MQTTSubcriberService(IDataProcessingPipeline pipeline,
            IOptions<MqttSettings> settings,
            ILogger<MQTTSubcriberService> logger)
        {
            _pipeline = pipeline;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MQTT Subscriber Service starting...");
            await CreateConnectedClient(stoppingToken);
           // Держим сервис активным
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                // Проверяем соединение
                if (!_mqttClient.IsConnected)
                {
                    _logger.LogWarning("MQTT client is not connected, attempting to reconnect...");
                    await CreateConnectedClient(stoppingToken);
                }
            }
        }

        private async Task CreateConnectedClient(CancellationToken stoppingToken)
        {
            try
            {
                // ИСПРАВЛЕНИЕ: Замени MqttClientFactory на MqttFactory
                var factory = new MqttClientFactory();
                _mqttClient = factory.CreateMqttClient();
                //нужно контролировать версию Mqtt у этого клиента версия 5 а у брокера 3.11 
                //решил проблему обновив версию у брокера до 5 с помощью флага

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_settings.Server, _settings.Port)
                    .WithCredentials(_settings.Username, _settings.Password)
                    .WithClientId($"{_settings.ClientId}-{Guid.NewGuid():N}")
                    .WithCleanSession()
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                    // .WithTlsOptions()
                    .Build();

                // ✅ ВАЖНО: Принудительно используем MQTT 3.1.1 можно и так и так даже лучше
                 //.WithProtocolVersion(MqttProtocolVersion.V311)

                // создаем методы которые будут делегатами замещающие основные функции 
                // подписка отписка и просмотр сообщений
                _mqttClient.ConnectedAsync += Subcribe;
                _mqttClient.DisconnectedAsync += async e =>
                {
                    _logger.LogWarning("Disconnected from MQTT broker. Reason: {Reason}", e.Reason);

                    if (e.Exception != null)
                    {
                        _logger.LogError(e.Exception, "MQTT connection error");
                    }

                    // Попытка переподключения
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await _mqttClient.ConnectAsync(options, stoppingToken);
                };
                _mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    //Читаем данные
                    var message = e.ApplicationMessage;
                    var payload = Encoding.UTF8.GetString(message.Payload);
                    //Console.WriteLine($"Received message on topic '{message.Topic}': {payload}");
                    //ОТПРАВЛЯЕМ Данные в обработчик 
                    //Console.WriteLine($"ПРИШЛО СООБЩЕНИЯ:{payload}");
                    await _pipeline.ProcessDataAsync(payload);
                };
                _logger.LogInformation("🔌 Connecting to MQTT broker at {Server}:{Port}", _settings.Server, _settings.Port);


                await _mqttClient.ConnectAsync(options, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to connect to MQTT broker");
                throw;
            }
        }
        private async Task Subcribe(MqttClientConnectedEventArgs e) 
        {
            _logger.LogInformation("✅ Successfully connected to MQTT broker");

            await _mqttClient!.SubscribeAsync(_settings.Topic);
            _logger.LogInformation("📡 Subscribed to topic: {Topic}", _settings.Topic);
        }
        public async Task Disconnect()
        {
            await _mqttClient.DisconnectAsync();
        }

    }
}
