namespace UniverseLabs.Messages;

public class OrderCreatedMessage : BaseMessage
{
    public long Id { get; set; }

    public long CustomerId { get; set; }

    public string DeliveryAddress { get; set; } = string.Empty;

    public long TotalPriceCents { get; set; }

    public string TotalPriceCurrency { get; set; } = string.Empty;

    public string OrderStatus { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public OrderItemUnit[] OrderItems { get; set; } = [];

    public override string RoutingKey => "order.created";
}
