using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Authentication;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Persistence.Dapper.PostgreSQL
{
    [ExcludeFromCodeCoverage]
    public class UserRepository : AbstractRepository, IUserRepository
    {
        private const string ReturningFields = " RETURNING id, username, role, inserted_at, inserted_by, updated_at, updated_by";

        private readonly ILogger<UserRepository> logger;
        private readonly DbDataSource dbDataSource;
        private readonly IAuthenticatedUserContext authenticatedUserContext;

        public UserRepository(ILogger<UserRepository> logger, DbDataSource dbDataSource, IAuthenticatedUserContext authenticatedUserContext) : base(logger)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
            this.authenticatedUserContext = authenticatedUserContext;
        }

        /// <summary>
        /// Inserts user into database.
        /// Throws exceptions in case of database constraints violation.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Hashed user password</param>
        /// <param name="role">User Role</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<User> AddAsync(string username, string password, string role, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO users(username, password, role, inserted_by) VALUES(@Username, @Password, @Role, @Inserted_by)" + ReturningFields;
            var parameters = new { username, password, role, inserted_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var createdRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstAsync<User>(command), cancellationToken);
            return createdRecord;
        }

        /// <summary>
        /// Delete user into database.
        /// Return true in case of existing record, return false in case of non-existing record.
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                var procedure = "delete_user";
                var parameters = new { userid = id };
                var command = new CommandDefinition(procedure, parameters, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

                var affectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.ExecuteAsync(command), cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                logger.LogDebug("User deleted", affectedRecords);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected exception user groups");
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }

        /// <summary>
        /// Return user data by id.
        /// Returns null in case of non-existing entity
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, username, role, inserted_at, inserted_by, updated_at, updated_by FROM users WHERE id = @id";
            var parameters = new { id };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var selectedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<User?>(command), cancellationToken);
            logger.LogDebug("Selected user {selectedRecord}", selectedRecord);
            return selectedRecord;
        }

        /// <summary>
        /// Return user data by id.
        /// Returns null in case of non-existing entity
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<IEnumerable<User>?> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, username, role, inserted_at, inserted_by, updated_at, updated_by FROM users";
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            var selectedRecords = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryAsync<User>(command), cancellationToken);
            logger.LogDebug("Returning {selectedRecord} users.", selectedRecords.Count());
            return selectedRecords;
        }

        /// <summary>
        /// Return user data by username.
        /// Returns null in case of non-existing entity.
        /// </summary>
        /// <param name="">Username</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "SELECT id, username, password, role, inserted_at, inserted_by, updated_at, updated_by FROM users WHERE username = @username";
            var parameters = new { username };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var selectedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<User?>(command), cancellationToken);
            logger.LogDebug("Selected user {selectedRecord}", selectedRecord);
            return selectedRecord;
        }

        /// <summary>
        /// Updates user into database.
        /// Returns null in case of non-existing user id.
        /// Throws exceptions in case of database errors and constraint violations.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<User?> UpdateAsync(int id, string password, string role, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "UPDATE users SET password = @password, role = @role, updated_at = @updated_at, updated_by = @updated_by WHERE id = @id" + ReturningFields;
            var parameters = new { id, password, role, updated_at = DateTime.UtcNow, updated_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var updatedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<User?>(command), cancellationToken);
            if (updatedRecord != null)
            {
                logger.LogDebug("Updated user {updatedRecord}", updatedRecord);
            }
            return updatedRecord;
        }

        /// <summary>
        /// Update user role into database.
        /// Returns null in case of non-existing user id.
        /// Throws exceptions in case of database errors and constraint violations.
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="password">password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<User?> UpdatePasswordAsync(int id, string password, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "UPDATE users SET password = @password, updated_at = @updated_at, updated_by = @updated_by WHERE id = @id" + ReturningFields;
            var parameters = new { id, password, updated_at = DateTime.UtcNow, updated_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var updatedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<User?>(command), cancellationToken);

            if (updatedRecord != null)
            {
                logger.LogDebug("Updated user {id} password.", id);
            }

            return updatedRecord;
        }

        /// <summary>
        /// Update user role into database.
        /// Returns null in case of non-existing user id.
        /// Throws exceptions in case of database errors and constraint violations.
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="role">Role</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<User?> UpdateRoleAsync(int id, string role, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "UPDATE users SET role = @role, updated_at = @updated_at, updated_by = @updated_by WHERE id = @id" + ReturningFields;
            var parameters = new { id, role, updated_at = DateTime.UtcNow, updated_by = authenticatedUserContext.GetUserName() };
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var updatedRecord = await ExecuteWithRetryOnTransientErrorAsync(() => connection.QueryFirstOrDefaultAsync<User?>(command), cancellationToken);

            if (updatedRecord != null)
            {
                logger.LogDebug("Updated user {id} role.", id);
            }

            return updatedRecord;
        }
    }

}
