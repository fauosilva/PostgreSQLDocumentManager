using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;

namespace ApplicationCore.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest createUserRequest, CancellationToken cancellationToken = default);                
        Task<UserResponse?> UpdatePasswordAsync(int userId, string password, CancellationToken cancellationToken = default);
        Task<UserResponse?> UpdateUserAsync(int userId, UpdateUserRequest updateUserRequest, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    }
}
