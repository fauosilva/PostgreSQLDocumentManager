namespace ApplicationCore.Interfaces.Repositories
{
    public interface IFileRepository
    {
        Task<Stream> DownloadFileAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> UploadFromStreamAsync(Stream fileStream, string? contentType, string key, string name, string category, string description, CancellationToken cancellationToken = default);
    }
}
