using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;

namespace ApplicationCore.Interfaces.Services
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string? contentType, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default);
    }
}
