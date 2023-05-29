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
    public class UserGroupsRepository : AbstractRepository, IUserGroupRepository
    {
        private const string ReturningFields = " RETURNING user_id, group_id, inserted_at, inserted_by, updated_at, updated_by";

        private readonly ILogger<UserGroupsRepository> logger;
        private readonly DbDataSource dbDataSource;
        private readonly IAuthenticatedUserContext authenticatedUserContext;

        public UserGroupsRepository(ILogger<UserGroupsRepository> logger, DbDataSource dbDataSource, IAuthenticatedUserContext authenticatedUserContext) : base(logger)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
            this.authenticatedUserContext = authenticatedUserContext;
        }

        /// <summary>
        /// Adds a usergroup database record. 
        /// </summary>
        /// <param name="groupId">Group id</param>
        /// <param name="userId">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created UserGroup record. Throws exceptions in case of database constraints violation.</returns>
        public async Task<UserGroup> AddUserToGroupAsync(int groupId, int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO user_groups(group_id, user_id, inserted_by) VALUES(@GroupId, @UserId, @inserted_by)" + ReturningFields;
            var parameters = new { groupId, userId, inserted_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstAsync<UserGroup>(command), cancellationToken);

            logger.LogDebug("User {userId} added to group {groupId} .", userId, groupId);
            return createdRecord;
        }

        /// <summary>
        /// Delete a usergroup database record.        
        /// </summary>
        /// <param name="groupId">Group id</param>
        /// <param name="userId">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return true in case of existing record, return false in case of non-existing record.</returns>
        public async Task<bool> RemoveUserFromGroupAsync(int groupId, int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "DELETE FROM user_groups WHERE group_id = @GroupId AND user_id = @UserId";
            var parameters = new { groupId, userId };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var affectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.ExecuteAsync(command), cancellationToken);
            logger.LogDebug("Number of records deleted {affectedRecords}", affectedRecords);
            return affectedRecords > 0;
        }
    }
}
