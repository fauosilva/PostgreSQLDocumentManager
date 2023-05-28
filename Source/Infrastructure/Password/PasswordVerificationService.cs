using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Password
{
    public class PasswordVerificationService : IPasswordVerificationService
    {
        private readonly ILogger<PasswordVerificationService> logger;
        private readonly IUserService userService;

        public PasswordVerificationService(ILogger<PasswordVerificationService> logger, IUserService userService)
        {
            this.logger = logger;
            this.userService = userService;
        }

        public async Task<bool> VerifyHashedPasswordAsync(int userId, string hashedPassword, string providedPassword, CancellationToken? cancellationToken = default)
        {
            var hashVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(new User() { Id = userId }, hashedPassword, providedPassword);

            switch (hashVerificationResult)
            {
                case PasswordVerificationResult.Failed: return false;
                case PasswordVerificationResult.Success: return true;
                case PasswordVerificationResult.SuccessRehashNeeded:
                    try
                    {
                        logger.LogInformation("Password hash was verified with success, attempting to rehash.");
                        await userService.UpdatePasswordAsync(userId, providedPassword);
                        logger.LogInformation("Password rehashed.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Password hash was verified with success, but was unable to rehash existing password.");
                    }
                    return true;

                default: return false;
            }
        }
    }
}
