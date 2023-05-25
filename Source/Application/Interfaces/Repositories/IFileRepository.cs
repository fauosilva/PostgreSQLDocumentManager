using ApplicationCore.Dtos.Requests;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IFileRepository
    {
        Task<bool> UploadFileAsync(Stream fileStream, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default);
    }
}
