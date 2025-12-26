namespace UniverseLabs.Messages;

public class OrderItemUnit
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int Quantity { get; set; }

    public string ProductTitle { get; set; } = string.Empty;

    public string ProductUrl { get; set; } = string.Empty;

    public long PriceCents { get; set; }

    public string PriceCurrency { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
