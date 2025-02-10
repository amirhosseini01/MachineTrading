using System.Linq.Expressions;

namespace MachineTrading.Repository.Base;

public interface IGenericRepository<T>
{
    Task AddAsync(T item, CancellationToken ct = default);
    Task AddRangeAsync(ICollection<T> items, CancellationToken ct = default);
    Task<T?> FindAsync(object id, CancellationToken ct = default);
    void Remove(T item);
    void RemoveRange(ICollection<T> items);
    Task SaveChangesAsync(CancellationToken ct = default);
    void UpdateRange(ICollection<T> items);
}