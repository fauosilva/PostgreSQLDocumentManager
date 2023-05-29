using ApplicationCore.Constants;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Authentication
{
    public class AuthenticatedUserContext : IAuthenticatedUserContext
    {
        private readonly IHttpContextAccessor httpContextAcessor;
        private int? userId;
        private string? userName;
        private string? role;

        public AuthenticatedUserContext(IHttpContextAccessor httpContext)
        {
            httpContextAcessor = httpContext;
        }

        public int GetUserId()
        {
            if (userId == null)
            {
                if (!int.TryParse(httpContextAcessor.HttpContext.User.FindFirstValue(UserClaimsConstants.UserId), out int id))
                    throw new ServiceException("Unable to find current user id");

                userId = id;
            }
            return userId.Value;
        }

        public string GetUserName()
        {
            return userName ??= httpContextAcessor.HttpContext.User.FindFirstValue(UserClaimsConstants.UserName)
                ?? throw new ServiceException("Unable to find current user name");
        }

        public string GetUserRole()
        {
            if (role == null)
            {
                var claimsPrincipal = httpContextAcessor.HttpContext.User;
                var userRole = claimsPrincipal.FindFirstValue(UserClaimsConstants.Role) ?? claimsPrincipal.FindFirstValue(ClaimsIdentity.DefaultRoleClaimType);
                if (!string.IsNullOrEmpty(userRole))
                {
                    role = userRole;                    
                }
            }
            return role ?? throw new ServiceException("Unable to find current user role");
        }
    }
}
