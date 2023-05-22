using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IBaseRepository<T, in TKeyType> where T : BaseEntity<TKeyType> where TKeyType : notnull
    {
        Task<T?> GetAsync(TKeyType id, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(TKeyType id, CancellationToken cancellationToken = default);
    }
}
