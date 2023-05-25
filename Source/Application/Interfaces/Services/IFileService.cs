using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Services
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadFileAsync(Stream fileStream, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default);
    }
}
