using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;

namespace ApplicationCore.Interfaces.Services
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest createUserRequest, CancellationToken? cancellationToken = default);
        Task<UpdateUserResponse?> UpdatePasswordAsync(int userId, string password, CancellationToken? cancellationToken = default);
        Task<UpdateUserResponse?> UpdateUserAsync(int userId, UpdateUserRequest updateUserRequest, CancellationToken? cancellationToken = default);
    }
}
