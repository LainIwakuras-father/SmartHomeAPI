namespace SmartHome.API.Settings
{
    public class MqttSettings
    {
        public string Server { get; set; } = "localhost";
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int ClientId { get; set; }
        public string Topic { get; set; }
    }
}
