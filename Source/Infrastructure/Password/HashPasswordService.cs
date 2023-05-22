using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Password
{
    public class HashPasswordService : IHashPasswordService
    {
        public string HashPassword(string password)
        {
            return new PasswordHasher<User>().HashPassword(new User(), password);
        }
    }
}
