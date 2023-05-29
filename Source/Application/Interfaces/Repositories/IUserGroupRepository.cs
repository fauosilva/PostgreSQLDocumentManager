using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IUserGroupRepository
    {
        Task<UserGroup> AddUserToGroupAsync(int groupId, int userId, CancellationToken cancellationToken = default);
        Task<bool> RemoveUserFromGroupAsync(int groupId, int userId, CancellationToken cancellationToken = default);
    }
}
