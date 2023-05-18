using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBaseRepository<T, in TKeyType> where T : BaseEntity<TKeyType>
    {
        Task<T> GetAsync(TKeyType id);
        Task<T> AddAsync(T entity);
        Task<int> DeleteAsync(T entity);
        Task<int> UpdateAsync(T entity);
    }
}
