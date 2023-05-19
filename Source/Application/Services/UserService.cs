using Application.Dtos.Requests;
using Application.Dtos.Responses;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> logger;
        private readonly IUserRepository userRepository;

        public UserService(ILogger<UserService> logger, IUserRepository userRepository)
        {
            this.logger = logger;
            this.userRepository = userRepository;            
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest createUserRequest)
        {
            //Todo: Inject
            var hashedPassword = new PasswordHasher().HashPassword(createUserRequest.Password);

            var userToInsert = new User() { Username = createUserRequest.Username, Password = hashedPassword, Role = createUserRequest.Role.ToString() };
            logger.LogDebug("Attempting to create user with Username {Username}", userToInsert.Username);
            
            try
            {
                var user = await this.userRepository.AddAsync(userToInsert);

                //Todo: Use Automapper
                return new CreateUserResponse()
                {
                    Id = user.Id,
                    Password = user.Password,
                    Role = user.Role,
                    InsertedAt = user.InsertedAt,
                    InsertedBy = user.InsertedBy,
                    UpdatedAt = user.UpdatedAt,
                    UpdatedBy = user.UpdatedBy,
                    Username = user.Username
                };
            }
            catch (Exception ex)
            {
                logger.LogError("Unable to create user.", ex);
                throw;
            }            
        }
    }
}
