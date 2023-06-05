using ApplicationCore.Interfaces.Authentication;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace PostgreSQLDocumentManager.Authorization
{
    [ExcludeFromCodeCoverage]
    public class DocumentDownloadAuthorizationHandler : AuthorizationHandler<UserGroupRequirement, int>
    {
        private readonly IDocumentPermissionRepository documentPermissionRepository;
        private readonly IAuthenticatedUserContext authenticatedUserContext;

        public DocumentDownloadAuthorizationHandler(IDocumentPermissionRepository documentPermissionRepository, IAuthenticatedUserContext authenticatedUserContext)
        {
            this.documentPermissionRepository = documentPermissionRepository;
            this.authenticatedUserContext = authenticatedUserContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       UserGroupRequirement requirement,
                                                       int resourceId)
        {
            var userRoleCanDownload = new HashSet<string>() { "Admin", "Manager" };
            if (userRoleCanDownload.Contains(authenticatedUserContext.GetUserRole()))
            {
                context.Succeed(requirement);
                return;
            }

            bool canDownload = await documentPermissionRepository.CanDownloadAsync(authenticatedUserContext.GetUserId(), resourceId);
            if (canDownload)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail(new AuthorizationFailureReason(this, "User does not belong to a group that have access to the document or does not have direct access to it"));
            }
        }
    }

#pragma warning disable S2094 // Classes should not be empty
    public class UserGroupRequirement : IAuthorizationRequirement { }
#pragma warning restore S2094 // Classes should not be empty
}
