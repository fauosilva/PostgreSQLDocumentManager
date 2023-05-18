using Application.Dtos.Requests;
using Application.Dtos.Responses;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest createUserRequest);
    }
}
