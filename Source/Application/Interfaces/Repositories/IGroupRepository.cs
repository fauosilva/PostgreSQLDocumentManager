using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IGroupRepository
    {
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Group?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Group>?> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Group?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<Group> AddAsync(string name, CancellationToken cancellationToken = default);
        Task<Group?> UpdateAsync(int id, string name, CancellationToken cancellationToken = default);
    }
}
