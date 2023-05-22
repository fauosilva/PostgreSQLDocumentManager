using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Services;

namespace ApplicationCore.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository userRepository;
        private readonly IPasswordVerificationService passwordVerificationService;

        public LoginService(IUserRepository userRepository, IPasswordVerificationService passwordVerificationService)
        {
            this.userRepository = userRepository;
            this.passwordVerificationService = passwordVerificationService;
        }

        public async Task<UserResponse?> AuthenticateUserAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        {
            var existingUser = await userRepository.GetByUsernameAsync(loginRequest.Username, cancellationToken);
            if (existingUser == null)
                return default;

            var success = await passwordVerificationService.VerifyHashedPasswordAsync(existingUser.Id, existingUser.Password, loginRequest.Password,
                cancellationToken);

            if (success)
            {
                return new UserResponse(existingUser);
            }

            return default;
        }
    }
}
