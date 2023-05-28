using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;

namespace ApplicationCore.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<DownloadDocumentResponse> DownloadFileAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken = default);
        Task<CreateDocumentResponse> UploadFileAsync(Stream fileStream, string? contentType, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default);
    }
}
