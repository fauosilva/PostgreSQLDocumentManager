using ApplicationCore.Constants;
using ApplicationCore.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Authentication
{
    public class AuthenticatedUserContext : IAuthenticatedUserContext
    {
        private readonly IHttpContextAccessor httpContextAcessor;
        private string? userId;
        private string? userName;
        private string? role;

        public AuthenticatedUserContext(IHttpContextAccessor httpContext)
        {
            httpContextAcessor = httpContext;
        }

        public string GetUserId()
        {
            return userId ??= httpContextAcessor.HttpContext.User.FindFirstValue(UserClaimsConstants.UserId) 
                ?? throw new Exception("Unable to find current user id");
        }

        public string GetUserName()
        {
            return userName ??= httpContextAcessor.HttpContext.User.FindFirstValue(UserClaimsConstants.UserName) 
                ?? throw new Exception("Unable to find current user name");
        }

        public string GetUserRole()
        {
            return role ??= httpContextAcessor.HttpContext.User.FindFirstValue(UserClaimsConstants.Role) 
                ?? throw new Exception("Unable to find current user role");
        }
    }
}
