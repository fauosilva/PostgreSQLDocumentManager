using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections;
using UnitTests.Extensions.FluentAssertions;

namespace UnitTests.ApplicationCore.Services
{
    public class UserServiceTests
    {
        private readonly ILogger<UserService> logger;
        private readonly Mock<IHashPasswordService> hashPasswordServiceMock;

        public UserServiceTests()
        {
            logger = NullLoggerFactory.Instance.CreateLogger<UserService>();
            hashPasswordServiceMock = new Mock<IHashPasswordService>();
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenDeleteUserAsyncReturnsTrue()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act            
            var result = await userService.DeleteUserAsync(1, cancellationToken);

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenDeleteUserAsyncReturnsFalse()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act            
            var result = await userService.DeleteUserAsync(1, cancellationToken);

            //Assert
            result.Should().BeFalse();
        }


        [Fact]
        public async Task GetUserAsync_ShouldReturnNull_WhenGetUserAsyncReturnsNull()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act            
            var result = await userService.GetUserAsync(1, cancellationToken);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserAsync_ShouldReturnUserResponse_WhenGetUserAsyncReturnsUser()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            var repositoryResponse = new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" };
            userRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(repositoryResponse);
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act            
            var result = await userService.GetUserAsync(1, cancellationToken);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(repositoryResponse,
                options => options.Excluding(f => f.Password)
                .WithAuditableMapping());
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnEmptyList_WhenGetAllAsyncReturnsNull()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act            
            var result = await userService.GetUsersAsync(cancellationToken);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUsersAsync_ShouldUserResponseList_WhenGetAllAsyncReturnsRecords()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            var response = new List<User>()
            {
                new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" },
                new User() { Id = 2, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" }
            };
            userRepository.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act            
            var result = await userService.GetUsersAsync(cancellationToken);

            //Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(response,
                options => options.Excluding(f => f.Password)
                .WithAuditableMapping());
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnServiceException_WhenErrorHappensInGetByUsernameAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Sample"));
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);
            var createUserRequest = new CreateUserRequest() { Username = "SampleSample", Password = "SampleSample", Role = Role.User };

            //Act
            Exception? returnedException = null;
            try
            {
                await userService.CreateUserAsync(createUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().NotBeNull();
            returnedException.Should().BeOfType<ServiceException>();
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnServiceException_WhenExistingUserIsReturnedByGetByUsernameAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new User());
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);
            var createUserRequest = new CreateUserRequest() { Username = "SampleSample", Password = "SampleSample", Role = Role.User };

            //Act
            Exception? returnedException = null;
            try
            {
                await userService.CreateUserAsync(createUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().NotBeNull();
            returnedException.Should().BeOfType<ServiceException>();
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnServiceException_WhenErrorHappensHashPasswordException()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            hashPasswordServiceMock.Setup(m => m.HashPassword(It.IsAny<string>())).Throws(new Exception("Sample"));
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);
            var createUserRequest = new CreateUserRequest() { Username = "SampleSample", Password = "SampleSample", Role = Role.User };

            //Act
            Exception? returnedException = null;
            try
            {
                await userService.CreateUserAsync(createUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().NotBeNull();
            returnedException.Should().BeOfType<ServiceException>();
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnServiceException_WhenErrorHappensInAddAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            userRepository.Setup(m => m.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);
            var createUserRequest = new CreateUserRequest() { Username = "SampleSample", Password = "SampleSample", Role = Role.User };

            //Act
            Exception? returnedException = null;
            try
            {
                await userService.CreateUserAsync(createUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().NotBeNull();
            returnedException.Should().BeOfType<ServiceException>();
        }


        [Fact]
        public async Task CreateUserAsync_ShouldReturnCreatedUser_WhenUserIsCreatedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            var response = new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" };
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            userRepository.Setup(m => m.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);
            var createUserRequest = new CreateUserRequest() { Username = "SampleSample", Password = "SampleSample", Role = Role.User };

            //Act
            Exception? returnedException = null;
            CreateUserResponse? result = null;
            try
            {
                result = await userService.CreateUserAsync(createUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(response,
                options => options.Excluding(f => f.Password)
                .WithAuditableMapping());
        }

        [Theory]
        [ClassData(typeof(UpdateUserTestErrorMocks))]
        public async Task UpdateUserAsync_ShouldReturnServiceException_WhenErrorHappens(UpdateUserRequest updateUserRequest, Mock<IUserRepository> userRepository, Mock<IHashPasswordService> hashPasswordServiceMock)
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act
            Exception? returnedException = null;
            try
            {
                await userService.UpdateUserAsync(1, updateUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().NotBeNull();
            returnedException.Should().BeOfType<ServiceException>();
        }

        [Theory]
        [InlineData("samplesample", Role.User)]
        [InlineData(null, Role.User)]
        [InlineData(null, Role.Manager)]
        [InlineData("samplesample", null)]
        public async Task UpdateUserAsync_ShouldReturnNull_WhenNonExistingUserIsAttempted(string? password, Role? role)
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var updateUserRequest = new UpdateUserRequest { Password = password, Role = role };
            var repositoryMock = new Mock<IUserRepository>();

            if (!string.IsNullOrEmpty(password) && role != null)
            {
                repositoryMock.Setup(m => m.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            }
            else if (!string.IsNullOrEmpty(password))
            {
                repositoryMock.Setup(m => m.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            }
            else
            {
                repositoryMock.Setup(m => m.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            }

            var hashPasswordService = new Mock<IHashPasswordService>();
            hashPasswordService.Setup(m => m.HashPassword(It.IsAny<string>())).Throws(new Exception("Sample"));

            var userService = new UserService(logger, repositoryMock.Object, hashPasswordServiceMock.Object);

            //Act
            Exception? returnedException = null;
            UserResponse? result = null;
            try
            {
                result = await userService.UpdateUserAsync(1, updateUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("samplesample", Role.User)]
        [InlineData(null, Role.User)]
        [InlineData(null, Role.Manager)]
        [InlineData("samplesample", null)]
        public async Task UpdateUserAsync_ShouldReturnUpdatedUser_WhenExistingUserIsUpdated(string? password, Role? role)
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var updateUserRequest = new UpdateUserRequest { Password = password, Role = role };
            var repositoryMock = new Mock<IUserRepository>();
            var updatedUser = new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = role?.ToString() ?? string.Empty, Username = "sample" };

            if (!string.IsNullOrEmpty(password) && role != null)
            {
                repositoryMock.Setup(m => m.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedUser);
            }
            else if (!string.IsNullOrEmpty(password))
            {
                repositoryMock.Setup(m => m.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedUser);
            }
            else
            {
                repositoryMock.Setup(m => m.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedUser);
            }

            var hashPasswordService = new Mock<IHashPasswordService>();
            hashPasswordService.Setup(m => m.HashPassword(It.IsAny<string>())).Returns("hashedPassword");

            var userService = new UserService(logger, repositoryMock.Object, hashPasswordServiceMock.Object);

            //Act
            Exception? returnedException = null;
            UserResponse? result = null;
            try
            {
                result = await userService.UpdateUserAsync(1, updateUserRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedUser,
               options => options.Excluding(f => f.Password)
               .WithAuditableMapping());
        }

        [Theory]
        [ClassData(typeof(UpdateUserPasswordTestErrorMocks))]
        public async Task UpdatePasswordUserAsync_ShouldReturnServiceException_WhenErrorHappens(UpdateUserRequest updateUserRequest, Mock<IUserRepository> userRepository, Mock<IHashPasswordService> hashPasswordServiceMock)
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userService = new UserService(logger, userRepository.Object, hashPasswordServiceMock.Object);

            //Act
            Exception? returnedException = null;
            try
            {
                await userService.UpdatePasswordAsync(1, updateUserRequest.Password!, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().NotBeNull();
            returnedException.Should().BeOfType<ServiceException>();
        }

        [Fact]
        public async Task UpdateUserPasswordAsync_ShouldReturnUpdatedUser_WhenExistingUserIsUpdated()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var updateUserRequest = new UpdateUserRequest { Password = "samplesample", Role = null };
            var repositoryMock = new Mock<IUserRepository>();

            repositoryMock.Setup(m => m.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);

            var hashPasswordService = new Mock<IHashPasswordService>();
            hashPasswordService.Setup(m => m.HashPassword(It.IsAny<string>())).Returns("hashedPassword");

            var userService = new UserService(logger, repositoryMock.Object, hashPasswordServiceMock.Object);

            //Act
            Exception? returnedException = null;
            UserResponse? result = null;
            try
            {
                result = await userService.UpdatePasswordAsync(1, updateUserRequest.Password, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserPasswordAsync_ShouldReturnNull_WhenNonExistingUserIsAttempted()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var updateUserRequest = new UpdateUserRequest { Password = "samplesample", Role = null };
            var updatedUser = new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample", Password = "samplesample", Updated_At = DateTime.UtcNow, Updated_By = "sample" };
            var repositoryMock = new Mock<IUserRepository>();

            repositoryMock.Setup(m => m.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedUser);

            var hashPasswordService = new Mock<IHashPasswordService>();
            hashPasswordService.Setup(m => m.HashPassword(It.IsAny<string>())).Returns("hashedPassword");

            var userService = new UserService(logger, repositoryMock.Object, hashPasswordServiceMock.Object);

            //Act
            Exception? returnedException = null;
            UserResponse? result = null;
            try
            {
                result = await userService.UpdatePasswordAsync(1, updateUserRequest.Password, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedUser,
              options => options.Excluding(f => f.Password)
              .WithAuditableMapping());
        }

        public class UpdateUserTestErrorMocks : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var repositoryMock = new Mock<IUserRepository>();
                repositoryMock.Setup(m => m.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Sample"));

                yield return new object[] {
                    new UpdateUserRequest(){ Password = "samplesample", Role = Role.User },
                    repositoryMock,
                    new Mock<IHashPasswordService>() };

                var repositoryMock1 = new Mock<IUserRepository>();
                repositoryMock1.Setup(m => m.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Sample"));

                yield return new object[] {
                    new UpdateUserRequest(){ Password = "samplesample" },
                    repositoryMock1,
                    new Mock<IHashPasswordService>() };

                var repositoryMock2 = new Mock<IUserRepository>();
                repositoryMock2.Setup(m => m.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Sample"));

                yield return new object[] {
                    new UpdateUserRequest(){ Role = Role.User },
                    repositoryMock2,
                    new Mock<IHashPasswordService>() };

                var hashPasswordService = new Mock<IHashPasswordService>();
                hashPasswordService.Setup(m => m.HashPassword(It.IsAny<string>())).Throws(new Exception("Sample"));

                yield return new object[] {
                    new UpdateUserRequest(){ Password= "samplesample" },
                    new Mock<IUserRepository>(),
                    hashPasswordService };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class UpdateUserPasswordTestErrorMocks : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var repositoryMock1 = new Mock<IUserRepository>();
                repositoryMock1.Setup(m => m.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Sample"));

                yield return new object[] {
                    new UpdateUserRequest(){ Password = "samplesample" },
                    repositoryMock1,
                    new Mock<IHashPasswordService>() };

                var hashPasswordService = new Mock<IHashPasswordService>();
                hashPasswordService.Setup(m => m.HashPassword(It.IsAny<string>())).Throws(new Exception("Sample"));

                yield return new object[] {
                    new UpdateUserRequest(){ Password= "samplesample" },
                    new Mock<IUserRepository>(),
                    hashPasswordService };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
