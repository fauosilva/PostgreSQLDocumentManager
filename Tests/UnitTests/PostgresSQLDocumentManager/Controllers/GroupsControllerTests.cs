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
    public class GroupsControllerTests
    {
        private readonly ILogger<GroupsController> logger;

        public GroupsControllerTests()
        {
            logger = NullLoggerFactory.Instance.CreateLogger<GroupsController>();
        }

        [Fact]
        public async Task GetGroups_ShouldReturnOkObjectResultWithEmptyList_WhenNoGroupsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupService = new Mock<IGroupService>();
            groupService.Setup(m => m.GetGroupsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<GroupResponse>());
            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.GetGroups(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.Should().BeAssignableTo<IEnumerable<GroupResponse>>();
            var returnedList = returnValue as IEnumerable<GroupResponse>;
            returnedList.Should().BeEmpty();
        }

        [Fact]
        public async Task GetGroups_ShouldReturnOkObjectResultGroupList_WhenGroupsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupService = new Mock<IGroupService>();
            groupService.Setup(m => m.GetGroupsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
                new List<GroupResponse>() {
                    new GroupResponse(new Group() {Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" })
                });
            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.GetGroups(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.Should().BeAssignableTo<IEnumerable<GroupResponse>>();
            var returnedList = returnValue as IEnumerable<GroupResponse>;
            returnedList.Should().NotBeEmpty();
            returnedList.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetGroup_ShouldReturnNotFound_WhenNoGroupsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupService = new Mock<IGroupService>();
            groupService.Setup(m => m.GetGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.GetGroup(1, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetGroup_ShouldReturnOkObjectResultGroup_WhenGroupsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupService = new Mock<IGroupService>();
            var controllerResponse = new GroupResponse(new Group() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" });
            groupService.Setup(m => m.GetGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(controllerResponse);
            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.GetGroup(1, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(controllerResponse);
        }

        [Fact]
        public async Task CreateGroup_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new CreateGroupRequest() { Name = string.Empty };
            var groupService = new Mock<IGroupService>();
            var controller = new GroupsController(logger, groupService.Object);
            controller.ModelState.AddModelError("Name", "Required");

            //Act            
            IActionResult actionResult = await controller.CreateGroup(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateGroup_ShouldReturnOkObjectResult_WhenGroupCreatedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new CreateGroupRequest() { Name = string.Empty };
            var groupService = new Mock<IGroupService>();
            var groupServiceResponse = new CreateGroupResponse(new Group() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" });
            groupService.Setup(m => m.CreateGroupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(groupServiceResponse);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.CreateGroup(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(groupServiceResponse);
        }

        [Fact]
        public async Task UpdateGroup_ShouldReturnNotFoundResult_WhenUpdateGroupAsyncReturnsNull()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UpdateGroupRequest() { Name = "sampleName" };
            var groupService = new Mock<IGroupService>();
            groupService.Setup(m => m.UpdateGroupAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.UpdateGroup(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateGroupGroup_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UpdateGroupRequest() { Name = string.Empty };
            var groupService = new Mock<IGroupService>();
            var controller = new GroupsController(logger, groupService.Object);
            controller.ModelState.AddModelError("Name", "Required");

            //Act            
            IActionResult actionResult = await controller.UpdateGroup(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateGroup_ShouldReturnOkObjectResult_WhenUpdateGroupAsyncReturnsUpdatedEntity()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UpdateGroupRequest() { Name = "SampleName" };
            var groupService = new Mock<IGroupService>();
            var groupServiceResponse = new GroupResponse(new Group() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "SampleName" });
            groupService.Setup(m => m.UpdateGroupAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => groupServiceResponse);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.UpdateGroup(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(groupServiceResponse);
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnNotFoundResult_WhenDeleteGroupsAsyncReturnsFalse()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = 1;
            var groupService = new Mock<IGroupService>();
            groupService.Setup(m => m.DeleteGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.DeleteGroup(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteGroup_ShouldNoContentResult_WhenUpdateGroupAsyncReturnsTrue()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = 1;
            var groupService = new Mock<IGroupService>();
            groupService.Setup(m => m.DeleteGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.DeleteGroup(request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task AddUser_ShouldReturnOkObjectResult_WhenUserAddedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UserGroupRequest() { UserId = 1 };
            var groupService = new Mock<IGroupService>();
            var groupServiceResponse = new CreateUserGroupResponse(new UserGroup { Group_Id = 1, User_Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "Sample" });
            groupService.Setup(m => m.AddUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(groupServiceResponse);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.AddUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(groupServiceResponse);
        }


        [Fact]
        public async Task AddUser_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UserGroupRequest() { UserId = 1 };
            var groupService = new Mock<IGroupService>();
            var controller = new GroupsController(logger, groupService.Object);
            controller.ModelState.AddModelError("Name", "Required");

            //Act            
            IActionResult actionResult = await controller.AddUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RemoveUser_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UserGroupRequest() { UserId = 1 };
            var groupService = new Mock<IGroupService>();
            var controller = new GroupsController(logger, groupService.Object);
            controller.ModelState.AddModelError("Name", "Required");

            //Act            
            IActionResult actionResult = await controller.RemoveUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RemoveUser_ShouldReturnNoContentResult_WhenUserRemovedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UserGroupRequest() { UserId = 1 };
            var groupService = new Mock<IGroupService>();            
            groupService.Setup(m => m.RemoveUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.RemoveUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NoContentResult>();            
        }

        [Fact]
        public async Task RemoveUser_ShouldReturnNotFoundResult_WhenUserNotRemovedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new UserGroupRequest() { UserId = 1 };
            var groupService = new Mock<IGroupService>();
            groupService.Setup(m => m.RemoveUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new GroupsController(logger, groupService.Object);

            //Act            
            IActionResult actionResult = await controller.RemoveUser(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }
    }
}
