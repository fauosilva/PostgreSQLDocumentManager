using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using PostgreSQLDocumentManager.Configuration;
using PostgreSQLDocumentManager.Controllers;

namespace UnitTests.PostgresSQLDocumentManager.Controllers
{
    public class AuthenticationControllerTests
    {
        public AuthenticationControllerTests()
        {
        }

        [Fact]
        public async Task Login_ShouldReturnOkObjectResult_WhenUserIsAuthenticated()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var loginService = new Mock<ILoginService>();
            JwtConfiguration jwtConfiguration = new()
            {
                Issuer = "sample",
                Audience = "audience",
                Key = "samplekeysamplekey"
            };

            var options = Options.Create(jwtConfiguration);

            loginService.Setup(m => m.AuthenticateUserAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserResponse(new User() { Id = 1, Username = "Sample", Role = Role.User.ToString() }));
            var controller = new AuthenticationController(loginService.Object, options);

            //Act            
            IActionResult actionResult = await controller.Login(
                new LoginRequest() { Username = "Sample", Password = "Sample" }, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var loginService = new Mock<ILoginService>();
            JwtConfiguration jwtConfiguration = new()
            {
                Issuer = "sample",
                Audience = "audience",
                Key = "samplekeysamplekey"
            };

            var options = Options.Create(jwtConfiguration);

            loginService.Setup(m => m.AuthenticateUserAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);
            var controller = new AuthenticationController(loginService.Object, options);

            //Act            
            IActionResult actionResult = await controller.Login(
                new LoginRequest() { Username = "Sample", Password = "Sample" }, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<UnauthorizedResult>();            
        }
    }
}
