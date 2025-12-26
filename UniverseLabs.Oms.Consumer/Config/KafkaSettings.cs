namespace UniverseLabs.Oms.Consumer.Config;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;

    public string GroupId { get; set; } = string.Empty;

    public string OmsOrderCreatedTopic { get; set; } = string.Empty;

    public string OmsOrderStatusChangedTopic { get; set; } = string.Empty;
}
