using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services
{
    public class UserService : IUserService
    {
        private const string UnexpectedErrorOnUpdate = "Unexpected error happened. Unable to update user.";
        private const string UnexpectedErrorOnInsert = "Unexpected error happened. Unable to create user.";

        private readonly ILogger<UserService> logger;
        private readonly IUserRepository userRepository;
        private readonly IHashPasswordService hashPasswordService;

        public UserService(ILogger<UserService> logger, IUserRepository userRepository, IHashPasswordService hashPasswordService)
        {
            this.logger = logger;
            this.userRepository = userRepository;
            this.hashPasswordService = hashPasswordService;            
        }

        public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        {
            var affectedEntities = await userRepository.DeleteAsync(id, cancellationToken);
            if (affectedEntities > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<UserResponse?> GetUserAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await userRepository.GetAsync(id, cancellationToken);
            if(user != null)
            {
                return new UserResponse(user);
            }

            return default;
        }

        public async Task<IEnumerable<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await userRepository.GetAllAsync(cancellationToken);
            if (users.Any())
            {
                return users.Where(item => item is not null).Select(user => new UserResponse(user!));
            }
            
            return new List<UserResponse>();
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest createUserRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingUser = await userRepository.GetByUsernameAsync(createUserRequest.Username, cancellationToken);
                if(existingUser != null)
                {                    
                    throw new ServiceException($"Unable to create user. Username {createUserRequest.Username} already exists.");
                }

                logger.LogDebug("Hashing provided user password.");
                var hashedPassword = hashPasswordService.HashPassword(createUserRequest.Password);

                logger.LogDebug("Attempting to create user with Username {Username}", createUserRequest.Username);
                var user = await userRepository.AddAsync(createUserRequest.Username, hashedPassword, createUserRequest.Role.ToString(), cancellationToken);

                logger.LogInformation("User successfully created. Id: {userId}, Username: {username}", user.Id, user.Username);
                return new CreateUserResponse(user);
            }
            catch (Exception ex)
            {                
                logger.LogError(ex, UnexpectedErrorOnInsert);
                throw new ServiceException(UnexpectedErrorOnInsert, ex);
            }
        }

        public async Task<UserResponse?> UpdateUserAsync(int userId, UpdateUserRequest updateUserRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                User? updatedUser = null;
                if (!string.IsNullOrEmpty(updateUserRequest.Password))
                {
                    logger.LogDebug("Hashing provided user password.");
                    var hashedPassword = hashPasswordService.HashPassword(updateUserRequest.Password);

                    if (updateUserRequest.Role.HasValue)
                    {
                        logger.LogDebug("Updating user password and role.");
                        updatedUser = await userRepository.UpdateAsync(userId, hashedPassword, updateUserRequest.Role.Value.ToString(), cancellationToken);
                    }
                    else
                    {
                        logger.LogDebug("Updating user password.");
                        updatedUser = await userRepository.UpdatePasswordAsync(userId, hashedPassword, cancellationToken);
                    }
                }
                else
                {
                    if (updateUserRequest.Role.HasValue)
                    {
                        logger.LogDebug("Updating user role.");
                        updatedUser = await userRepository.UpdateRoleAsync(userId, updateUserRequest.Role.Value.ToString(), cancellationToken);
                    }
                }

                if (updatedUser != null)
                {
                    logger.LogInformation("User updated. User Id: {userId}, Username: {username}", updatedUser.Id, updatedUser.Username);
                    return new UserResponse(updatedUser);
                }

                return default;
            }
            catch (Exception ex)
            {                
                logger.LogError(ex, UnexpectedErrorOnUpdate);
                throw new ServiceException(UnexpectedErrorOnUpdate, ex);
            }
        }

        public async Task<UserResponse?> UpdatePasswordAsync(int userId, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogDebug("Hashing provided user password.");
                var hashedPassword = hashPasswordService.HashPassword(password);

                logger.LogDebug("Updating user password.");
                var user = await userRepository.UpdatePasswordAsync(userId, hashedPassword, cancellationToken);
                if (user != null)
                {
                    logger.LogInformation("User password updated. User Id: {userId}, Username: {username}", user.Id, user.Username);
                    return new UserResponse(user);
                }

                return default;
            }
            catch (Exception ex)
            {               
                logger.LogError(ex, UnexpectedErrorOnUpdate);
                throw new ServiceException(UnexpectedErrorOnUpdate, ex);
            }
        }        
    }
}
