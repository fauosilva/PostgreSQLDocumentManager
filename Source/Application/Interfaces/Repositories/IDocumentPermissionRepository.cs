using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IDocumentPermissionRepository
    {
        Task<bool> CanDownloadAsync(int userId, int documentId, CancellationToken cancellationToken = default);
        Task<DocumentPermission> AddUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<DocumentPermission> AddGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default);
    }
}
