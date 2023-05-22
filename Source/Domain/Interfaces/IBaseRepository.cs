using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBaseRepository<T, in TKeyType> where T : BaseEntity<TKeyType>
    {
        Task<T> GetAsync(TKeyType id, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(TKeyType id, CancellationToken cancellationToken = default);        
    }
}
