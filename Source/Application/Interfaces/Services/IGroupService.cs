using ApplicationCore.Dtos.Responses;

namespace ApplicationCore.Interfaces.Services
{
    public interface IGroupService
    {
        Task<CreateUserGroupResponse> AddUserAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<GroupResponse?> GetGroupAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<GroupResponse>> GetGroupsAsync(CancellationToken cancellationToken = default);
        Task<CreateGroupResponse> CreateGroupAsync(string name, CancellationToken cancellationToken = default);
        Task<GroupResponse?> UpdateGroupAsync(int id, string name, CancellationToken cancellationToken = default);
        Task<bool> DeleteGroupAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> RemoveUserAsync(int id, int userId, CancellationToken cancellationToken = default);
    }
}
