namespace WebApi.Config;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string OmsOrderCreatedTopic { get; set; } = string.Empty;

    public string OmsOrderStatusChangedTopic { get; set; } = string.Empty;
}
