using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Authentication;
using ApplicationCore.Interfaces.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

namespace Infrastructure.Persistence.Dapper.PostgreSQL
{
    [ExcludeFromCodeCoverage]
    public class GroupRepository : AbstractRepository, IGroupRepository
    {
        private const string ReturningFields = " RETURNING id, name, inserted_at, inserted_by, updated_at, updated_by";

        private readonly ILogger<GroupRepository> logger;
        private readonly DbDataSource dbDataSource;
        private readonly IAuthenticatedUserContext authenticatedUserContext;

        public GroupRepository(ILogger<GroupRepository> logger, DbDataSource dbDataSource, IAuthenticatedUserContext authenticatedUserContext) : base(logger)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
            this.authenticatedUserContext = authenticatedUserContext;
        }

        /// <summary>
        /// Inserts group into database. 
        /// </summary>
        /// <param name="name">Group name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created group. Throws exceptions in case of database constraints violation.</returns>
        public async Task<Group> AddAsync(string name, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO groups(name, inserted_by) VALUES(@Name, @Inserted_by)" + ReturningFields;
            var parameters = new { name, inserted_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstAsync<Group>(command), cancellationToken);
            logger.LogDebug("Group {groupname} created on database with id {id}", createdRecord.Name, createdRecord.Id);
            return createdRecord;
        }

        /// <summary>
        /// Delete group into database.        
        /// </summary>
        /// <param name="id">Group id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return true in case of existing record, return false in case of non-existing record.</returns>
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                string sqlUserGroups = "DELETE FROM user_groups WHERE group_id = @id";
                var parametersUserGroups = new { id };
                var commandUserGroups = new CommandDefinition(sqlUserGroups, parametersUserGroups, transaction, cancellationToken: cancellationToken);

                var deletedGroupAssociations = await ExecuteWithRetryOnTransientErrorAsync(() => connection.ExecuteAsync(commandUserGroups), cancellationToken);
                logger.LogDebug("Number of associations deleted {affectedRecords}", deletedGroupAssociations);

                string sqlPermissions = "DELETE FROM document_permissions WHERE group_id = @id";
                var parametersPermissions = new { id };
                var commandPermissions = new CommandDefinition(sqlPermissions, parametersPermissions, transaction, cancellationToken: cancellationToken);

                var deletedDocumentPermissions = await ExecuteWithRetryOnTransientErrorAsync(() => connection.ExecuteAsync(commandPermissions), cancellationToken);
                logger.LogDebug("Number of permissions deleted {affectedRecords}", deletedDocumentPermissions);

                string sql = "DELETE FROM groups WHERE id = @id";
                var parameters = new { id };
                var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);

                var affectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.ExecuteAsync(command), cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                logger.LogDebug("Number of records deleted {affectedRecords}", affectedRecords);
                return affectedRecords > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected exception deleting user groups");
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }

        /// <summary>
        /// Return group data by id from database        
        /// </summary>
        /// <param name="id">Group id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Returns a group if exists, otherwise returns null</returns>
        public async Task<Group?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, name, inserted_at, inserted_by, updated_at, updated_by FROM groups WHERE id = @id";
            var parameters = new { id };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var selectedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<Group?>(command), cancellationToken);
            if (selectedRecord != null)
            {
                logger.LogDebug("Selected group {selectedRecord}", selectedRecord?.Name);
            }
            return selectedRecord;
        }

        /// <summary>
        /// Return groups from database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return all groups from database. Returns null in case of non-existing entities</returns>
        public async Task<IEnumerable<Group>?> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT g.id, g.name, g.inserted_at, g.inserted_by, g.updated_at, g.updated_by ," +
                "u.user_id, u.group_id, u.inserted_at, u.inserted_by, u.updated_at, u.updated_by " +
                "FROM groups g left outer join user_groups u ON g.id = u.group_id";
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            Dictionary<int, Group> resultCache = new();
            _ = await ExecuteWithRetryOnTransientErrorAsync(() =>
                connection.QueryAsync<Group, UserGroup, Group>(command, (group, userGroup) =>
                {
                    if (!resultCache.ContainsKey(group.Id))
                    {
                        resultCache.Add(group.Id, group);
                    }
                    Group cachedParent = resultCache[group.Id];
                    cachedParent.Users ??= new List<UserGroup>();
                    cachedParent.Users.Add(userGroup);

                    return cachedParent;
                }, splitOn: "user_id"), cancellationToken);


            logger.LogDebug("Returning {selectedRecord} groups.", resultCache.Values.Count);
            return resultCache.Values;
        }

        /// <summary>
        /// Retrieve group by name from database.
        /// </summary>
        /// <param name="name">Group name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns> Return group data by name. Returns null in case of non-existing entity.</returns>
        public async Task<Group?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, name, inserted_at, inserted_by, updated_at, updated_by FROM groups WHERE name = @Name";
            var parameters = new { name };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var selectedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<Group?>(command), cancellationToken);
            if (selectedRecord != null)
            {
                logger.LogDebug("Selected group {selectedRecord}", selectedRecord.Name);
            }
            return selectedRecord;
        }

        /// <summary>
        /// Updates group into database.
        /// </summary>
        /// <param name="id">Group id</param>
        /// <param name="name">Group new name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Returns updated record. Returns null in case of non-existing group id. Throws exceptions in case of database errors and constraint violations.</returns>
        public async Task<Group?> UpdateAsync(int id, string name, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "UPDATE groups SET name = @Name, updated_at = @updated_at, updated_by = @updated_by WHERE id = @id" + ReturningFields;
            var parameters = new { id, name, updated_at = DateTime.UtcNow, updated_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var updatedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<Group?>(command), cancellationToken);
            if (updatedRecord != null)
            {
                logger.LogDebug("Updated group {updatedRecord}", updatedRecord.Name);
            }
            return updatedRecord;
        }
    }
}
