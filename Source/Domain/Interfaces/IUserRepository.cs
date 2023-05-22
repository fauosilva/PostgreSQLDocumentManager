using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository : IBaseRepository<User, int>
    {
        Task<User> AddAsync(string UserName, string Password, string Role, CancellationToken cancellationToken = default);
    }
}

