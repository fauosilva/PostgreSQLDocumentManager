using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Authentication;
using ApplicationCore.Interfaces.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Persistence.Dapper.PostgreSQL
{
    [ExcludeFromCodeCoverage]
    public class DocumentRepository : AbstractRepository, IDocumentRepository
    {
        private const string ReturningFields = " RETURNING id, name, description, category, keyname, uploaded, inserted_at, inserted_by, updated_at, updated_by";

        private readonly ILogger<DocumentRepository> logger;
        private readonly DbDataSource dbDataSource;
        private readonly IAuthenticatedUserContext authenticatedUserContext;

        public DocumentRepository(ILogger<DocumentRepository> logger, DbDataSource dbDataSource, IAuthenticatedUserContext authenticatedUserContext) : base(logger)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
            this.authenticatedUserContext = authenticatedUserContext;
        }

        /// <summary>
        /// Inserts document into database.
        /// Throws exceptions in case of database constraints violation.
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="description">File description</param>
        /// <param name="category">File category</param>
        /// <param name="keyname">File keyname</param>
        /// <param name="uploaded">Boolean indicating that the file have been ploaded</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Document data stored on the database.</returns>
        public async Task<Document> AddAsync(string name, string description, string category, string keyname, bool uploaded = false, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO documents(name, description, category, keyname, uploaded, inserted_by) VALUES(@Name, @Description, @Category, @Keyname, @Uploaded, @Inserted_by)" + ReturningFields;
            var parameters = new { name, description, category, keyname, uploaded, inserted_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstAsync<Document>(command), cancellationToken);
            return createdRecord;
        }

        /// <summary>
        /// Returns document by name, description and category
        /// Returns null in case of non-existing document
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="category"></param>
        /// <param name="cancellationToken">Cancelletion Token</param>
        /// <returns>Document data stored on the database or null in case of non-existing document</returns>
        public async Task<Document?> GetDocumentByNameDescriptionAndCategoryAsync(string name, string description, string category, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, name, description, category, keyname, uploaded, inserted_at, inserted_by, updated_at, updated_by FROM documents WHERE name = @Name AND description = @Description AND category = @Category";
            var parameters = new { name, description, category };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<Document?>(command), cancellationToken);
            return createdRecord;
        }

        /// <summary>
        /// Update document uploaded status into database.     
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="uploaded">Uploaded flag</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Returns null in case of non-existing document id. Throws exceptions in case of database errors and constraint violations</returns>
        public async Task<Document?> UpdateUploadedStatusAsync(int id, bool uploaded, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "UPDATE documents SET uploaded = @uploaded, updated_at = @updated_at, updated_by = @updated_by WHERE id = @id" + ReturningFields;
            var parameters = new { id, uploaded, updated_at = DateTime.UtcNow, updated_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var updatedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<Document?>(command), cancellationToken);

            if (updatedRecord != null)
            {
                logger.LogDebug("Updated document {id} uploaded status {uploaded}.", id, uploaded);
            }

            return updatedRecord;
        }

        /// <summary>
        /// Return document data by id.
        /// Returns null in case of non-existing entity
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Document if exists, otherwise null</returns>
        public async Task<Document?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, name, description, category, keyname, uploaded, inserted_at, inserted_by, updated_at, updated_by FROM documents WHERE id = @id";
            var parameters = new { id };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var selectedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<Document?>(command), cancellationToken);
            logger.LogDebug("Selected document {selectedRecord}", selectedRecord);
            return selectedRecord;
        }

        /// <summary>
        /// Return all documents from database.        
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of documents. Returns null in case of non-existing entities</returns>
        public async Task<IEnumerable<Document>?> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT d.id, d.name, d.description, d.category, d.keyname, d.uploaded, d.inserted_at, d.inserted_by, d.updated_at, d.updated_by, " +
                "p.id, p.document_id, p.user_id, p.group_id, p.inserted_at, p.inserted_by, p.updated_at, p.updated_by " +
                "FROM documents d LEFT OUTER JOIN document_permissions p ON d.id = p.document_id";
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            Dictionary<int, Document> resultCache = new();
            _ = await ExecuteWithRetryOnTransientErrorAsync(() =>
                connection.QueryAsync<Document, DocumentPermission, Document>(command, (document, permission) =>
                {
                    if (!resultCache.ContainsKey(document.Id))
                    {
                        resultCache.Add(document.Id, document);
                    }
                    Document cachedParent = resultCache[document.Id];
                    cachedParent.Permissions ??= new List<DocumentPermission>();
                    cachedParent.Permissions.Add(permission);

                    return cachedParent;
                }, splitOn: "id"), cancellationToken);


            logger.LogDebug("Returning {selectedRecord} documents.", resultCache.Values.Count);
            return resultCache.Values;
        }
    }
}
