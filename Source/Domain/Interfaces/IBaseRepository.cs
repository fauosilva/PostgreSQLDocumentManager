using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBaseRepository<T, in TKeyType> where T : BaseEntity<TKeyType>
    {
        Task<T> GetAsync(TKeyType id, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    }
}
