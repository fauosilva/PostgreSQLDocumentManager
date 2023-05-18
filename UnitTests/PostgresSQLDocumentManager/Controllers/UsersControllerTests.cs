using Application.Dtos.Requests;
using Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PostgreSQLDocumentManager.Controllers;

namespace UnitTests.PostgresSQLDocumentManager.Controllers
{
    public class UsersControllerTests
    {
        private readonly ILogger<UsersController> logger;

        public UsersControllerTests()
        {
            logger = NullLoggerFactory.Instance.CreateLogger<UsersController>();
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var request = new CreateUserRequest() { Password = string.Empty, Username = string.Empty };
            var userService = new Mock<IUserService>();
            var controller = new UsersController(logger, userService.Object);
            controller.ModelState.AddModelError("Password", "Required");

            //Act            
            IActionResult actionResult = await controller.CreateUser(request);

            //Assert

            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
