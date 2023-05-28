using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> AddAsync(string name, string description, string category, string keyname, bool uploaded = false, CancellationToken cancellationToken = default);
    }
}
