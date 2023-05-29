using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> AddAsync(string name, string description, string category, string keyname, bool uploaded = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<Document>?> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Document?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<Document?> GetDocumentByNameDescriptionAndCategoryAsync(string name, string description, string category, CancellationToken cancellationToken = default);
        Task<Document?> UpdateUploadedStatusAsync(int id, bool uploaded, CancellationToken cancellationToken = default);
    }
}
