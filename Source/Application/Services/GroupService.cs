using ApplicationCore.Dtos.Responses;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services
{
    public class GroupService : IGroupService
    {
        private const string UnexpectedErrorOnUpdate = "Unexpected error happened. Unable to update group.";
        private const string UnexpectedErrorOnInsert = "Unexpected error happened. Unable to create group.";

        private readonly ILogger<GroupService> logger;
        private readonly IGroupRepository groupRepository;
        private readonly IUserGroupRepository userGroupRepository;

        public GroupService(ILogger<GroupService> logger, IGroupRepository groupRepository, IUserGroupRepository userGroupRepository)
        {
            this.logger = logger;
            this.groupRepository = groupRepository;
            this.userGroupRepository = userGroupRepository;
        }

        public async Task<bool> DeleteGroupAsync(int id, CancellationToken cancellationToken = default)
        {
            return await groupRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<GroupResponse?> GetGroupAsync(int id, CancellationToken cancellationToken = default)
        {
            var group = await groupRepository.GetAsync(id, cancellationToken);
            if (group != null)
            {
                return new GroupResponse(group);
            }

            return default;
        }

        public async Task<IEnumerable<GroupResponse>> GetGroupsAsync(CancellationToken cancellationToken = default)
        {
            var groups = await groupRepository.GetAllAsync(cancellationToken);
            if (groups != null && groups.Any())
            {
                return groups.Select(group => new GroupResponse(group));
            }

            return new List<GroupResponse>();
        }

        public async Task<CreateGroupResponse> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingGroup = await groupRepository.GetByNameAsync(name, cancellationToken);
                if (existingGroup != null)
                {
                    throw new ServiceException($"Unable to create group. A group with the same name {name} already exists.");
                }
                logger.LogDebug("Attempting to create group with name {name}", name);
                var group = await groupRepository.AddAsync(name, cancellationToken);

                logger.LogInformation("Group successfully created. Id: {id}, name: {name}", group.Id, group.Name);
                return new CreateGroupResponse(group);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, UnexpectedErrorOnInsert);
                throw new ServiceException(UnexpectedErrorOnInsert, ex);
            }
        }

        public async Task<GroupResponse?> UpdateGroupAsync(int id, string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var group = await groupRepository.UpdateAsync(id, name, cancellationToken);
                if (group != null)
                {
                    logger.LogInformation("Group updated. Id: {id}, name: {name}", group.Id, group.Name);
                    return new GroupResponse(group);
                }

                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, UnexpectedErrorOnUpdate);
                throw new ServiceException(UnexpectedErrorOnUpdate, ex);
            }
        }

        public async Task<CreateUserGroupResponse> AddUserAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            _ = await groupRepository.GetAsync(id, cancellationToken) ?? throw new ServiceException($"Group with {id} does not exist.");
            var result = await userGroupRepository.AddUserToGroupAsync(id, userId, cancellationToken);
            return new CreateUserGroupResponse(result);
        }

        public async Task<bool> RemoveUserAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            _ = await groupRepository.GetAsync(id, cancellationToken) ?? throw new ServiceException($"Group with {id} does not exist.");
            return await userGroupRepository.RemoveUserFromGroupAsync(id, userId, cancellationToken);
        }
    }
}
