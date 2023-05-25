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
        private const string ReturningFields = " RETURNING id, name, description, category, keyname, inserted_at, inserted_by, updated_at, updated_by";

        private readonly ILogger<UserRepository> logger;
        private readonly DbDataSource dbDataSource;

        public DocumentRepository(ILogger<UserRepository> logger, DbDataSource dbDataSource) : base(logger)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
        }

        /// <summary>
        /// Inserts document into database.
        /// Throws exceptions in case of database constraints violation.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Hashed user password</param>
        /// <param name="role">User Role</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<Document> AddAsync(string name, string description, string category, string keyname, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO document(name, description, category, keyname, inserted_by) VALUES(@Name, @Description, @Category, @Keyname, @Inserted_by)" + ReturningFields;
            var parameters = new { name, description, category, keyname, inserted_by = "Sample" };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstAsync<Document>(command), cancellationToken);
            return createdRecord;
        }
    }

}
