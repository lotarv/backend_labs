using Backend.DAL.Models;

namespace Backend.DAL.Interfaces;

public interface IOrderRepository
{
    Task<V1OrderDal[]> BulkInsert(V1OrderDal[] model, CancellationToken token);

    Task<V1OrderDal[]> Query(QueryOrdersDalModel model, CancellationToken token);

    Task<long[]> UpdateStatus(long[] ids, string status, DateTimeOffset updatedAt, CancellationToken token);
}
