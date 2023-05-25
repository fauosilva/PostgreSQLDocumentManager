using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Persistence.Files
{
    public class FileService : IFileService
    {
        public Task<bool> UploadFileAsync(string storageName, string filePath, string? keyName = null)
        {
            throw new NotImplementedException();
        }
    }
}
