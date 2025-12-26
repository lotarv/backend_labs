namespace WebApi.Config;

public class RabbitMqSettings
{
    public string HostName { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Exchange { get; set; } = string.Empty;

    public ExchangeMapping[] ExchangeMappings { get; set; } = [];

    public class ExchangeMapping
    {
        public string Queue { get; set; } = string.Empty;

        public string RoutingKeyPattern { get; set; } = string.Empty;
    }
}
