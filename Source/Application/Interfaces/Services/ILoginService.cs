using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;

namespace ApplicationCore.Interfaces.Services
{
    public interface ILoginService
    {
        Task<UserResponse?> AuthenticateUserAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default);
    }
}
