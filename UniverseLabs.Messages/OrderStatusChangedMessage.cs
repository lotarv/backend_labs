namespace UniverseLabs.Messages;

public class OrderStatusChangedMessage : BaseMessage
{
    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public long[] OrderItemIds { get; set; } = [];

    public string OrderStatus { get; set; } = string.Empty;

    public override string RoutingKey => "order.status.changed";
}
