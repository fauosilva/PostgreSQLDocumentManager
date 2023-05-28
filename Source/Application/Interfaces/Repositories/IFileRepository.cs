using ApplicationCore.Dtos.Requests;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IFileRepository
    {
        Task<bool> UploadFileAsync(Stream fileStream, string? contentType, string key, string name, string category, string description, CancellationToken cancellationToken = default);
        Task<bool> UploadLargeFileAsync(Stream fileStream, string? contentType, string key, string name, string category, string description, CancellationToken cancellationToken = default);
    }
}
