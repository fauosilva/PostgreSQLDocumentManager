using ApplicationCore.Dtos.Requests;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Services;
using FluentAssertions;
using Moq;

namespace UnitTests.ApplicationCore.Services
{
    public class LoginServiceTests
    {
        private readonly Mock<IPasswordVerificationService> passwordVerificationService;

        public LoginServiceTests()
        {
            passwordVerificationService = new Mock<IPasswordVerificationService>();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnUserResponse_WhenUserExistsAndIsAuthenticated()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User() { Id = 1, Password = "hashedPass", Role = "User", Username = "sample" });

            passwordVerificationService.Setup(m => m.VerifyHashedPasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var loginService = new LoginService(userRepository.Object, passwordVerificationService.Object);

            //Act            
            var result = await loginService.AuthenticateUserAsync(new LoginRequest() { Username = "sample", Password = "sample" }, cancellationToken);

            //Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenUserDoesNotExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User() { Id = 1, Password = "hashedPass", Role = "User", Username = "sample" });

            passwordVerificationService.Setup(m => m.VerifyHashedPasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var loginService = new LoginService(userRepository.Object, passwordVerificationService.Object);

            //Act            
            var result = await loginService.AuthenticateUserAsync(new LoginRequest() { Username = "sample", Password = "sample" }, cancellationToken);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenUserIsNotAuthenticated()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            var loginService = new LoginService(userRepository.Object, passwordVerificationService.Object);

            //Act            
            var result = await loginService.AuthenticateUserAsync(new LoginRequest() { Username = "sample", Password = "sample" }, cancellationToken);

            //Assert
            result.Should().BeNull();
        }
    }
}
