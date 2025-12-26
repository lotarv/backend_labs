using Backend.BLL.Models;
using Backend.DAL.Interfaces;
using Backend.DAL.Models;

namespace Backend.BLL.Services;

public class AuditLogOrderService(IAuditLogOrderRepository auditLogOrderRepository)
{
    public async Task<int> BatchInsert(AuditLogOrderUnit[] orders, CancellationToken token)
    {
        var now = DateTimeOffset.UtcNow;
        var models = orders.Select(x => new V1AuditLogOrderDal
        {
            OrderId = x.OrderId,
            OrderItemId = x.OrderItemId,
            CustomerId = x.CustomerId,
            OrderStatus = x.OrderStatus,
            CreatedAt = now,
            UpdatedAt = now
        }).ToArray();

        var res = await auditLogOrderRepository.BulkInsert(models, token);
        return res.Length;
    }
}
