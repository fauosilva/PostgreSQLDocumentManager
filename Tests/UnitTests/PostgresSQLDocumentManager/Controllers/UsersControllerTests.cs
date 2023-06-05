using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PostgreSQLDocumentManager.Controllers;
using UnitTests.Extensions;

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
        public async Task GetUsers_ShouldReturnOkObjectResultWithEmptyList_WhenNoUsersExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userService = new Mock<IUserService>();
            userService.Setup(m => m.GetUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserResponse>());
            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.GetUsers(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.Should().BeAssignableTo<IEnumerable<UserResponse>>();
            var returnedList = returnValue as IEnumerable<UserResponse>;
            returnedList.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUsers_ShouldReturnOkObjectResultUserList_WhenUsersExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userService = new Mock<IUserService>();
            userService.Setup(m => m.GetUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
                new List<UserResponse>() {
                    new UserResponse(new User() {Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" })
                });
            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.GetUsers(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.Should().BeAssignableTo<IEnumerable<UserResponse>>();
            var returnedList = returnValue as IEnumerable<UserResponse>;
            returnedList.Should().NotBeEmpty();
            returnedList.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenNoUsersExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userService = new Mock<IUserService>();
            userService.Setup(m => m.GetUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.GetUser(1, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetUser_ShouldReturnOkObjectResultUser_WhenUsersExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var userService = new Mock<IUserService>();
            var controllerResponse = new UserResponse(new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" });
            userService.Setup(m => m.GetUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(controllerResponse);
            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.GetUser(1, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(controllerResponse);            
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new CreateUserRequest() { Password = string.Empty, Username = string.Empty };
            var userService = new Mock<IUserService>();
            var controller = new UsersController(logger, userService.Object);
            controller.ModelState.AddModelError("Password", "Required");

            //Act            
            IActionResult actionResult = await controller.CreateUser(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateUser_ShouldReturnOkObjectResult_WhenUserCreatedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new CreateUserRequest() { Password = string.Empty, Username = string.Empty };
            var userService = new Mock<IUserService>();
            var userServiceResponse = new CreateUserResponse(new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" });
            userService.Setup(m => m.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userServiceResponse);

            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.CreateUser(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(userServiceResponse);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFoundResult_WhenUpdateUserAsyncReturnsNull()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UpdateUserRequest() { Password = "samplePassword", Role = null };
            var userService = new Mock<IUserService>();
            userService.Setup(m => m.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.UpdateUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateUserUser_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UpdateUserRequest() { Password = string.Empty, Role = null };
            var userService = new Mock<IUserService>();
            var controller = new UsersController(logger, userService.Object);
            controller.ModelState.AddModelError("Password", "Required");

            //Act            
            IActionResult actionResult = await controller.UpdateUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOkObjectResult_WhenUpdateUserAsyncReturnsUpdatedEntity()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UpdateUserRequest() { Password = "samplePassword", Role = null };
            var userService = new Mock<IUserService>();
            var userServiceResponse = new UserResponse(new User() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Role = "User", Username = "sample" });
            userService.Setup(m => m.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => userServiceResponse);

            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.UpdateUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(userServiceResponse);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFoundResult_WhenDeleteUsersAsyncReturnsFalse()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = 1;
            var userService = new Mock<IUserService>();
            userService.Setup(m => m.DeleteUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.DeleteUser(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteUser_ShouldNoContentResult_WhenUpdateUserAsyncReturnsTrue()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = 1;
            var userService = new Mock<IUserService>();
            userService.Setup(m => m.DeleteUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new UsersController(logger, userService.Object);

            //Act            
            IActionResult actionResult = await controller.DeleteUser(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NoContentResult>();
        }
    }
}
