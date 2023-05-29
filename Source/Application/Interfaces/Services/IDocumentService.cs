using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;

namespace ApplicationCore.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<CreatePermissionResponse> CreateUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<CreatePermissionResponse> CreateGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default);
        Task<DownloadDocumentResponse> DownloadFileAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken = default);
        Task<CreateDocumentResponse> UploadFileAsync(Stream fileStream, string? contentType, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default);
    }
}
