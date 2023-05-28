using ApplicationCore.Dtos.Requests;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IFileRepository
    {       
        Task<bool> UploadUnkownFileSizeFromStream(Stream fileStream, string? contentType, string key, string name, string category, string description, CancellationToken cancellationToken = default);
    }
}
