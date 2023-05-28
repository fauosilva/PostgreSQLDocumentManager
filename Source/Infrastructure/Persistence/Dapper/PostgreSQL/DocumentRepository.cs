using ApplicationCore.Entities;
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

        public DocumentRepository(ILogger<DocumentRepository> logger, DbDataSource dbDataSource) : base(logger)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
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
            var parameters = new { name, description, category, keyname, uploaded, inserted_by = "Sample" };
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
        /// Returns document by keyname
        /// Returns null in case of non-existing document
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="category"></param>
        /// <param name="cancellationToken">Cancelletion Token</param>
        /// <returns>Document data stored on the database or null in case of non-existing document</returns>
        public async Task<Document?> GetDocumentByKeyNameAsync(string keyName, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            //Todo: Add Index
            string sql = "SELECT id, name, description, category, keyname, uploaded, inserted_at, inserted_by, updated_at, updated_by FROM documents WHERE keyname = @Keyname";
            var parameters = new { keyName };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<Document?>(command), cancellationToken);
            return createdRecord;
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
    }

}
