using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UnitTests.Extensions.FluentAssertions;

namespace UnitTests.ApplicationCore.Services
{
    public class GroupServiceTests
    {
        private readonly ILogger<GroupService> logger;

        public GroupServiceTests()
        {
            logger = NullLoggerFactory.Instance.CreateLogger<GroupService>();
        }

        [Fact]
        public async Task DeleteGroupAsync_ShouldReturnTrue_WhenDeleteGroupAsyncReturnsTrue()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act            
            var result = await groupService.DeleteGroupAsync(1, cancellationToken);

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteGroupAsync_ShouldReturnFalse_WhenDeleteGroupAsyncReturnsFalse()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act            
            var result = await groupService.DeleteGroupAsync(1, cancellationToken);

            //Assert
            result.Should().BeFalse();
        }


        [Fact]
        public async Task GetGroupAsync_ShouldReturnNull_WhenGetGroupAsyncReturnsNull()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act            
            var result = await groupService.GetGroupAsync(1, cancellationToken);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetGroupAsync_ShouldReturnGroupResponse_WhenGetGroupAsyncReturnsGroup()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            var repositoryResponse = new Group() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" };
            groupRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(repositoryResponse);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act            
            var result = await groupService.GetGroupAsync(1, cancellationToken);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(repositoryResponse, options => options.WithAuditableMapping());
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenGetAllAsyncReturnsNull()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act            
            var result = await groupService.GetGroupsAsync(cancellationToken);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldGroupResponseList_WhenGetAllAsyncReturnsRecords()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            var response = new List<Group>()
            {
                new Group() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" },
                new Group() { Id = 2, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" }
            };
            groupRepository.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act            
            var result = await groupService.GetGroupsAsync(cancellationToken);

            //Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(response, options => options.WithAuditableMapping());
        }

        [Fact]
        public async Task CreateGroupAsync_ShouldReturnServiceException_WhenErrorHappensInGetByGroupnameAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Sample"));
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);
            var createGroupRequest = new CreateGroupRequest() { Name = "SampleSample" };

            //Act
            Exception? returnedException = null;
            try
            {
                await groupService.CreateGroupAsync(createGroupRequest.Name, cancellationToken);
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
        public async Task CreateGroupAsync_ShouldReturnServiceException_WhenExistingGroupIsReturnedByGetByGroupnameAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Group());
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);
            var createGroupRequest = new CreateGroupRequest() { Name = "SampleSample" };

            //Act
            Exception? returnedException = null;
            try
            {
                await groupService.CreateGroupAsync(createGroupRequest.Name, cancellationToken);
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
        public async Task CreateGroupAsync_ShouldReturnServiceException_WhenErrorHappensInAddAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            groupRepository.Setup(m => m.AddAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);
            var createGroupRequest = new CreateGroupRequest() { Name = "SampleSample" };

            //Act
            Exception? returnedException = null;
            try
            {
                await groupService.CreateGroupAsync(createGroupRequest.Name, cancellationToken);
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
        public async Task CreateGroupAsync_ShouldReturnCreatedGroup_WhenGroupIsCreatedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            var response = new Group() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" };
            groupRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            groupRepository.Setup(m => m.AddAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);
            var createGroupRequest = new CreateGroupRequest() { Name = "SampleSample" };

            //Act
            Exception? returnedException = null;
            CreateGroupResponse? result = null;
            try
            {
                result = await groupService.CreateGroupAsync(createGroupRequest.Name, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(response,
                options => options.Excluding(f => f.Users)
                .WithAuditableMapping());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Exception")]
        public async Task UpdateGroupAsync_ShouldReturnServiceException_WhenErrorHappens(object? returnType)
        {
            //Arrange
            UpdateGroupRequest updateGroupRequest = new UpdateGroupRequest() { Name = "sample" };
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            var cancellationToken = CancellationToken.None;

            if (returnType == null)
            {
                groupRepository.Setup(m => m.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(null);
            }
            else
            {
                groupRepository.Setup(m => m.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());
            }

            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act
            Exception? returnedException = null;
            try
            {
                await groupService.UpdateGroupAsync(1, updateGroupRequest.Name, cancellationToken);
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
        public async Task UpdateGroupAsync_ShouldReturnUpdatedGroup_WhenExistingGroupIsUpdated()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var updateGroupRequest = new UpdateGroupRequest { Name = "Sample" };
            var userGroupRepository = new Mock<IUserGroupRepository>();
            var repositoryMock = new Mock<IGroupRepository>();
            var updatedGroup = new Group() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "Sample" };

            repositoryMock.Setup(m => m.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).
                ReturnsAsync(updatedGroup);

            var groupService = new GroupService(logger, repositoryMock.Object, userGroupRepository.Object);

            //Act
            Exception? returnedException = null;
            GroupResponse? result = null;
            try
            {
                result = await groupService.UpdateGroupAsync(1, updateGroupRequest.Name, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedGroup,
               options => options.Excluding(f => f.Users)
               .WithAuditableMapping());
        }


        [Fact]
        public async Task UpdateGroupAsync_ShouldReturnNull_WhenNonExistingGroupIsUpdated()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var updateGroupRequest = new UpdateGroupRequest { Name = "Sample" };
            var userGroupRepository = new Mock<IUserGroupRepository>();
            var repositoryMock = new Mock<IGroupRepository>();

            repositoryMock.Setup(m => m.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).
                ReturnsAsync(() => null);

            var groupService = new GroupService(logger, repositoryMock.Object, userGroupRepository.Object);

            //Act
            Exception? returnedException = null;
            GroupResponse? result = null;
            try
            {
                result = await groupService.UpdateGroupAsync(1, updateGroupRequest.Name, cancellationToken);
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
        public async Task AddUserAsync_ShouldReturnCreatedUserGroup_WhenUserGroupIsCreatedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            var response = new UserGroup() { Group_Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", User_Id = 1 };
            groupRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Group());
            userGroupRepository.Setup(m => m.AddUserToGroupAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);


            //Act
            Exception? returnedException = null;
            CreateUserGroupResponse? result = null;
            try
            {
                result = await groupService.AddUserAsync(1, 1, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(response, options =>
                options.WithAuditableMapping()
                .WithMapping<CreateUserGroupResponse>(m => m.Group_Id, s => s.GroupId)
                .WithMapping<CreateUserGroupResponse>(m => m.User_Id, s => s.UserId));
        }

        [Fact]
        public async Task AddUserAsync_ShouldReturnServiceException_WhenUserGroupDoesNotExist()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act
            Exception? returnedException = null;
            CreateUserGroupResponse? result = null;
            try
            {
                result = await groupService.AddUserAsync(1, 1, cancellationToken);
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
        public async Task RemoveUserAsync_ShouldReturnTrue_WhenUserGroupIsDeletedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Group());
            userGroupRepository.Setup(m => m.RemoveUserFromGroupAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act
            Exception? returnedException = null;
            bool? result = null;
            try
            {
                result = await groupService.RemoveUserAsync(1, 1, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeTrue();
        }

        [Fact]
        public async Task RemoveUserAsyncc_ShouldReturnServiceException_WhenUserGroupDoesNotExist()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var groupRepository = new Mock<IGroupRepository>();
            var userGroupRepository = new Mock<IUserGroupRepository>();
            groupRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var groupService = new GroupService(logger, groupRepository.Object, userGroupRepository.Object);

            //Act
            Exception? returnedException = null;
            bool? result = null;
            try
            {
                result = await groupService.RemoveUserAsync(1, 1, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().NotBeNull();
            returnedException.Should().BeOfType<ServiceException>();
        }
    }
}
