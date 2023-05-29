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

        public DocumentRepository(ILogger<DocumentRepository> logger, DbDataSource dbDataSource, IAuthenticatedUserContext authenticatedUserContext ) : base(logger)
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
        /// Returns null in case of non-existing document id.
        /// Throws exceptions in case of database errors and constraint violations.
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="uploaded">Uploaded flag</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
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
        /// <returns>Document</returns>
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
        /// Returns null in case of non-existing entities
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of documents</returns>
        public async Task<IEnumerable<Document>?> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, name, description, category, keyname, uploaded, inserted_at, inserted_by, updated_at, updated_by FROM documents";
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            var selectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryAsync<Document>(command), cancellationToken);
            logger.LogDebug("Returning {selectedRecord} documents.", selectedRecords.Count());
            return selectedRecords;
        }

        public async Task<bool> CanDownloadAsync(int userId, int documentId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT 1 FROM document_permissions WHERE document_permissions.user_id = @UserId AND document_permissions.document_id = @DocumentId " +
                "UNION " +
                "SELECT 1 from user_groups INNER JOIN document_permissions on document_permissions.group_id = user_groups.group_id WHERE document_permissions.document_id = @DocumentId AND user_groups.user_id = @UserId";
            var parameters = new { userId, documentId };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var selectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryAsync<int>(command), cancellationToken);
            logger.LogDebug("User {userId} permission check to download document {documentId} returned: {selectedRecords}.", userId, documentId, selectedRecords.Any());

            return selectedRecords.Any();            
        }
    }
}
