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
    public class DocumentPermissionRepository : AbstractRepository, IDocumentPermissionRepository
    {
        private const string ReturningFields = " RETURNING id, document_id, user_id, group_id, inserted_at, inserted_by, updated_at, updated_by";

        private readonly ILogger<DocumentPermissionRepository> logger;
        private readonly DbDataSource dbDataSource;
        private readonly IAuthenticatedUserContext authenticatedUserContext;

        public DocumentPermissionRepository(ILogger<DocumentPermissionRepository> logger, DbDataSource dbDataSource, IAuthenticatedUserContext authenticatedUserContext) : base(logger)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
            this.authenticatedUserContext = authenticatedUserContext;
        }
        
        public async Task<bool> CanDownloadAsync(int userId, int documentId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT 1 FROM document_permissions WHERE document_permissions.user_id = @UserId AND document_permissions.document_id = @DocumentId";
            var parameters = new { userId, documentId };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var selectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<int?>(command), cancellationToken);

            if (selectedRecords.HasValue)
            {
                logger.LogDebug("User {userId} has permission to download document {documentId}", userId, documentId);
                return true;
            }

            string groupSql = "SELECT 1 from user_groups INNER JOIN document_permissions on document_permissions.group_id = user_groups.group_id WHERE document_permissions.document_id = @DocumentId AND user_groups.user_id = @UserId";

            var command2 = new CommandDefinition(groupSql, parameters, cancellationToken: cancellationToken);
            var selectedRecords2 = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<int?>(command2), cancellationToken);

            if (selectedRecords2.HasValue)
            {
                logger.LogDebug("User {userId} belongs to a group that has permission to download document {documentId}.", userId, documentId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Inserts document permission into database. 
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="groupId">Group id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created document permission. Throws exceptions in case of database constraints violation.</returns>
        public async Task<DocumentPermission> AddGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO document_permissions(document_id, group_id, inserted_by) VALUES(@Id, @GroupId, @inserted_by)" + ReturningFields;
            var parameters = new { id, groupId, inserted_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstAsync<DocumentPermission>(command), cancellationToken);

            logger.LogDebug("Document permission for document {documentId} created to group {groupId} on database with id {id}", id, groupId, createdRecord.Id);
            return createdRecord;
        }

        /// <summary>
        /// Inserts document permission into database. 
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="userId">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created document permission. Throws exceptions in case of database constraints violation.</returns>
        public async Task<DocumentPermission> AddUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO document_permissions(document_id, user_id, inserted_by) VALUES(@Id, @UserId, @inserted_by)" + ReturningFields;
            var parameters = new { id, userId, inserted_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstAsync<DocumentPermission>(command), cancellationToken);

            logger.LogDebug("Document permission for document {documentId} created to user {userId} on database with id {id}", id, userId, createdRecord.Id);
            return createdRecord;
        }

        /// <summary>
        /// Delete document permission into database.        
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="groupId">Group id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return true in case of existing record, return false in case of non-existing record.</returns>
        public async Task<bool> DeleteGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "DELETE FROM document_permissions WHERE document_id = @Id AND group_id = @GroupId";
            var parameters = new { id, groupId };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var affectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.ExecuteAsync(command), cancellationToken);
            logger.LogDebug("Number of records deleted {affectedRecords}", affectedRecords);
            return affectedRecords > 0;
        }

        /// <summary>
        /// Delete document permission into database.        
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="userId">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return true in case of existing record, return false in case of non-existing record.</returns>
        public async Task<bool> DeleteUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "DELETE FROM document_permissions WHERE document_id = @Id AND user_id = @UserId";
            var parameters = new { id, userId };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var affectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.ExecuteAsync(command), cancellationToken);
            logger.LogDebug("Number of records deleted {affectedRecords}", affectedRecords);
            return affectedRecords > 0;
        }    
    }
}
