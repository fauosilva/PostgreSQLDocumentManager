using ApplicationCore.Interfaces.Authentication;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace PostgreSQLDocumentManager.Authorization
{
    public class DocumentDownloadAuthorizationHandler : AuthorizationHandler<UserGroupRequirement, int>
    {
        private readonly IDocumentRepository documentRepository;
        private readonly IAuthenticatedUserContext authenticatedUserContext;

        public DocumentDownloadAuthorizationHandler(IDocumentRepository documentRepository, IAuthenticatedUserContext authenticatedUserContext)
        {
            this.documentRepository = documentRepository;
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

            bool canDownload = await documentRepository.CanDownloadAsync(authenticatedUserContext.GetUserId(), resourceId);
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
