using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Infrastructure.Persistence.Dapper.PostgreSQL
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> logger;
        private readonly DbDataSource dbDataSource;

        public UserRepository(ILogger<UserRepository> logger, DbDataSource dbDataSource)
        {
            this.logger = logger;
            this.dbDataSource = dbDataSource;
        }

        public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            await using var connection = await dbDataSource.OpenConnectionAsync(cancellationToken);

            string sql = "INSERT INTO Users(Username, Password, InsertedAt, InsertedBy, UpdatedAt, UpdatedBy) VALUES(@Username, @Password, @InsertedAt, @InsertedBy)";            
            
            //Todo: Replace with a stored procedure with audit functionality.
            var parameters = new { entity.Username, entity.Password, InsertedAt = DateTime.UtcNow, InsertedBy = "Sample"};
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            var affectedRecords = connection.ExecuteAsync(command);

            logger.LogDebug("Affected Records {affectedRecords}", affectedRecords);

            return entity;

            //var createdUser = await connection.QueryFirstAsync<User>("AddUser",commandType: CommandType.StoredProcedure);
            //return createdUser;
        }

        public Task<int> DeleteAsync(User entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(User entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
