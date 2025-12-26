using Backend.DAL.Models;

namespace Backend.DAL.Interfaces;

public interface IAuditLogOrderRepository
{
    Task<V1AuditLogOrderDal[]> BulkInsert(V1AuditLogOrderDal[] model, CancellationToken token);
}
