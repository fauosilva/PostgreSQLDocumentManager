using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;

namespace ApplicationCore.Interfaces
{
    public interface IUserRepository : IBaseRepository<User, int>
    {
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User> AddAsync(string username, string password, string role, CancellationToken cancellationToken = default);        
        Task<User?> UpdateAsync(int id, string password, string role, CancellationToken cancellationToken = default);
        Task<User?> UpdatePasswordAsync(int id, string password, CancellationToken cancellationToken = default);
        Task<User?> UpdateRoleAsync(int id, string role, CancellationToken cancellationToken = default);        
    }
}

