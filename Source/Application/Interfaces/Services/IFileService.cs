namespace ApplicationCore.Interfaces.Services
{
    public interface IFileService
    {
        Task<bool> UploadFileAsync(string storageName, string filePath, string? keyName = null);
    }
}
