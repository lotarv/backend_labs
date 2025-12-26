namespace UniverseLabs.Messages;

public class OrderStatusChangedMessage
{
    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public long[] OrderItemIds { get; set; } = [];

    public string OrderStatus { get; set; } = string.Empty;
}
