namespace Backend.BLL.Models;

public class AuditLogOrderUnit
{
    public long OrderId { get; set; }

    public long OrderItemId { get; set; }

    public long CustomerId { get; set; }

    public string OrderStatus { get; set; } = string.Empty;
}
