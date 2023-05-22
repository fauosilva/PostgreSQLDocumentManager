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
        private readonly ILogger<UserService> logger;
        private readonly IUserRepository userRepository;
        private readonly IHashPasswordService hashPasswordService;

        public UserService(ILogger<UserService> logger, IUserRepository userRepository, IHashPasswordService hashPasswordService)
        {
            this.logger = logger;
            this.userRepository = userRepository;
            this.hashPasswordService = hashPasswordService;
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest createUserRequest, CancellationToken? cancellationToken = default)
        {
            try
            {
                logger.LogDebug("Hashing provided user password.");
                var hashedPassword = hashPasswordService.HashPassword(createUserRequest.Password);

                logger.LogDebug("Attempting to create user with Username {Username}", createUserRequest.Username);
                var user = await userRepository.AddAsync(createUserRequest.Username, hashedPassword, createUserRequest.Role.ToString());

                logger.LogInformation("User successfully created. Id: {userId}, Username: {username}", user.Id, user.Username);
                return new CreateUserResponse(user);
            }
            catch (Exception ex)
            {
                var message = "Unexpected error happened. Unable to create user.";
                logger.LogError(ex, message);
                throw new ServiceException(message, ex);
            }
        }

        public async Task<UpdateUserResponse?> UpdateUserAsync(int userId, UpdateUserRequest updateUserRequest, CancellationToken? cancellationToken = default)
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
                        updatedUser = await userRepository.UpdateAsync(userId, hashedPassword, updateUserRequest.Role.Value.ToString());
                    }
                    else
                    {
                        logger.LogDebug("Updating user password.");
                        updatedUser = await userRepository.UpdatePasswordAsync(userId, hashedPassword);
                    }
                }
                else
                {
                    if (updateUserRequest.Role.HasValue)
                    {
                        logger.LogDebug("Updating user role.");
                        updatedUser = await userRepository.UpdateRoleAsync(userId, updateUserRequest.Role.Value.ToString());
                    }
                }

                if (updatedUser != null)
                {
                    logger.LogInformation("User updated. User Id: {userId}, Username: {username}", updatedUser.Id, updatedUser.Username);
                    return new UpdateUserResponse(updatedUser);
                }

                return default;
            }
            catch (Exception ex)
            {
                var message = "Unexpected error happened. Unable to update user.";
                logger.LogError(ex, message);
                throw new ServiceException(message, ex);
            }
        }

        public async Task<UpdateUserResponse?> UpdatePasswordAsync(int userId, string password, CancellationToken? cancellationToken = default)
        {
            try
            {
                logger.LogDebug("Hashing provided user password.");
                var hashedPassword = hashPasswordService.HashPassword(password);

                logger.LogDebug("Updating user password.");
                var user = await userRepository.UpdatePasswordAsync(userId, hashedPassword);
                if (user != null)
                {
                    logger.LogInformation("User password updated. User Id: {userId}, Username: {username}", user.Id, user.Username);
                    return new UpdateUserResponse(user);
                }

                return default;
            }
            catch (Exception ex)
            {
                var message = "Unexpected error happened. Unable to update user password.";
                logger.LogError(ex, message);
                throw new ServiceException(message, ex);
            }
        }
    }
}
