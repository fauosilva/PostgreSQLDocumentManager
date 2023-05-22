namespace ApplicationCore.Interfaces.Services
{
    public interface IPasswordVerificationService
    {
        Task<bool> VerifyHashedPasswordAsync(int userId, string hashedPassword, string providedPassword, CancellationToken? cancellationToken = default);
    }
}
