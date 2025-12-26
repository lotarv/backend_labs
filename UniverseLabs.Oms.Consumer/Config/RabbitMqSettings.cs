namespace UniverseLabs.Oms.Consumer.Config;

public class RabbitMqSettings
{
    public string HostName { get; set; } = string.Empty;

    public int Port { get; set; }

    public TopicSettingsUnit OrderCreated { get; set; } = new();

    public TopicSettingsUnit OrderStatusChanged { get; set; } = new();

    public class TopicSettingsUnit
    {
        public string Queue { get; set; } = string.Empty;

        public ushort BatchSize { get; set; }

        public int BatchTimeoutSeconds { get; set; }
    }
}
