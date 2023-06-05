using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PostgreSQLDocumentManager.Configuration;
using PostgreSQLDocumentManager.Controllers;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using UnitTests.Extensions;

namespace UnitTests.PostgresSQLDocumentManager.Controllers
{
    public class DocumentsControllerTests
    {
        private readonly ILogger<DocumentsController> logger;

        public DocumentsControllerTests()
        {
            logger = NullLoggerFactory.Instance.CreateLogger<DocumentsController>();
        }

        [Fact]
        public async Task GetDocuments_ShouldReturnOkObjectResultWithEmptyList_WhenNoDocumentsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());
            documentService.Setup(m => m.GetDocumentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DocumentResponse>());
            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.GetDocuments(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.Should().BeAssignableTo<IEnumerable<DocumentResponse>>();
            var returnedList = returnValue as IEnumerable<DocumentResponse>;
            returnedList.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDocuments_ShouldReturnOkObjectResultDocumentList_WhenDocumentsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());
            documentService.Setup(m => m.GetDocumentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
                new List<DocumentResponse>() {
                    new DocumentResponse(new Document() {Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample" })
                });
            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.GetDocuments(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.Should().BeAssignableTo<IEnumerable<DocumentResponse>>();
            var returnedList = returnValue as IEnumerable<DocumentResponse>;
            returnedList.Should().NotBeEmpty();
            returnedList.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDocument_ShouldReturnForbid_WhenUserDoesNotHaveAuthorization()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            authorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object?>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.DownloadDocument(1, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetDocument_ShouldReturnNotFound_WhenNoDocumentsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            authorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object?>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success);

            documentService.Setup(m => m.DownloadFileAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.DownloadDocument(1, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetDocument_ShouldReturnFileStreamResult_WhenDocumentsExists()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());
            var controllerResponse = new DownloadDocumentResponse(new MemoryStream(), "sample", "image/png");

            authorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object?>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success);

            documentService.Setup(m => m.DownloadFileAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(controllerResponse);
            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.DownloadDocument(1, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<FileStreamResult>();
        }

        [Fact]
        public async Task CreatePermission_ShouldReturnOkObjectResult_WhenPermissionCreatedToUserWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PermissionRequest() { UserId = 1 };
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            var documentServiceResponse = new CreatePermissionResponse(new DocumentPermission { Document_Id = 1, User_Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "Sample" });
            documentService.Setup(m => m.CreateUserPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(documentServiceResponse);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.CreatePermission(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(documentServiceResponse);
        }

        [Fact]
        public async Task CreatePermission_ShouldReturnOkObjectResult_WhenPermissionCreatedToGroupWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PermissionRequest() { GroupId = 1 };
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            var documentServiceResponse = new CreatePermissionResponse(new DocumentPermission { Document_Id = 1, Group_Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "Sample" });
            documentService.Setup(m => m.CreateGroupPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(documentServiceResponse);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.CreatePermission(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
            var returnValue = (actionResult as OkObjectResult)?.Value;
            returnValue.AssertControllerReturn(documentServiceResponse);
        }


        [Fact]
        public async Task CreatePermission_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PermissionRequest();
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);
            controller.ModelState.AddModelError("GroupId", "Required");

            //Act            
            IActionResult actionResult = await controller.CreatePermission(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeletePermission_ShouldReturnBadRequest_WhenInvalidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PermissionRequest();
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);
            controller.ModelState.AddModelError("GroupId", "Required");

            //Act            
            IActionResult actionResult = await controller.DeletePermission(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeletePermission_ShouldReturnNoContentResult_WhenPermissionRemovedFromUserWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PermissionRequest() { UserId = 1 };
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            documentService.Setup(m => m.DeleteUserPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.DeletePermission(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeletePermission_ShouldReturnNoContentResult_WhenPermissionRemovedFromGroupWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PermissionRequest() { GroupId = 1 };
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            documentService.Setup(m => m.DeleteGroupPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.DeletePermission(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NoContentResult>();
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(null, 1)]
        public async Task DeletePermission_ShouldReturnNotFoundResult_WhenPermissionNotRemovedWithSuccess(int? userId, int? groupId)
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());
            PermissionRequest? request = null;

            if (userId == null)
            {
                request = new PermissionRequest() { GroupId = groupId };
            }
            else
            {
                request = new PermissionRequest() { UserId = userId };
            }

            var documentService = new Mock<IDocumentService>();
            documentService.Setup(m => m.DeleteGroupPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            documentService.Setup(m => m.DeleteUserPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            //Act            
            IActionResult actionResult = await controller.DeletePermission(1, request, cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateFile_ShouldReturnOkObjectResult_WhenValidRequestParametersAreSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var configuration = new FileUploadConfiguration() { MimeTypes = new HashSet<string>() { "image/png" } };
            var options = Options.Create(configuration);

            documentService.Setup(m => m.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<FileUploadRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateDocumentResponse()
                {
                    Id = 1,
                    Category = "Category",
                    Description = "Description",
                    Name = "Name",
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "Sample",
                    KeyName = $"{DateTime.UtcNow}Name"
                });
            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);
            MultipartFormDataContent multiPartFormData = CreateMultiPartFormData("image/png");

            var HttpContext = new Mock<HttpContext>();
            HttpContext.Setup(m => m.Request.Body).Returns(multiPartFormData.ReadAsStream());
            HttpContext.Setup(m => m.Request.ContentType).Returns("multipart/form-data; boundary=\"------\"");

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = HttpContext.Object
            };

            //Act            
            IActionResult actionResult = await controller.CreateDocument(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<OkObjectResult>();
        }

        [Theory]
        [InlineData("image/png", "image/jpg")]
        [InlineData("image/jpg", "image/png")]
        [InlineData("image/png", "image/png")]
        public async Task CreateFile_ShouldReturnProblem_WhenInvalidValidRequestParametersAreSent(string configuredMimeTypes, string fileMimeType)
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var configuration = new FileUploadConfiguration() { MimeTypes = new HashSet<string>() { configuredMimeTypes } };
            var options = Options.Create(configuration);

            documentService.Setup(m => m.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<FileUploadRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);
            MultipartFormDataContent multiPartFormData = CreateMultiPartFormData(fileMimeType);

            var HttpContext = new Mock<HttpContext>();
            HttpContext.Setup(m => m.Request.Body).Returns(multiPartFormData.ReadAsStream());
            HttpContext.Setup(m => m.Request.ContentType).Returns("multipart/form-data; boundary=\"------\"");

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = HttpContext.Object
            };

            //Act            
            IActionResult actionResult = await controller.CreateDocument(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ObjectResult>();
            (actionResult as ObjectResult)?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task CreateFile_ShouldReturnProblem_WhenExceptionIsThrown()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var options = Options.Create(new FileUploadConfiguration());

            documentService.Setup(m => m.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<FileUploadRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);
            MultipartFormDataContent multiPartFormData = CreateMultiPartFormData();

            var HttpContext = new Mock<HttpContext>();
            HttpContext.Setup(m => m.Request.Body).Returns(multiPartFormData.ReadAsStream());
            HttpContext.Setup(m => m.Request.ContentType).Returns("multipart/form-data; boundary=\"------\"");

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = HttpContext.Object
            };

            //Act            
            IActionResult actionResult = await controller.CreateDocument(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<ObjectResult>();
            (actionResult as ObjectResult)?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task CreateFile_ShouldReturnBadRequest_WhenInvalidContentTypeIsSent()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var documentService = new Mock<IDocumentService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var configuration = new FileUploadConfiguration() { MimeTypes = new HashSet<string>() { "image/png" } };
            var options = Options.Create(configuration);

            var controller = new DocumentsController(logger, documentService.Object, authorizationService.Object, options);

            var streamContent = new StreamContent(new MemoryStream());
            streamContent.Headers.Add("Content-Type", "image/png");
            streamContent.Headers.Add("Content-Disposition", $"form-data; name=\"name\"; filename=\"filename.png\"");

            var multiPartFormData = new MultipartFormDataContent("------")
            {
                streamContent
            };

            var HttpContext = new Mock<HttpContext>();
            HttpContext.Setup(m => m.Request.Body).Returns(multiPartFormData.ReadAsStream());
            HttpContext.Setup(m => m.Request.ContentType).Returns("application/json");

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = HttpContext.Object
            };

            //Act            
            IActionResult actionResult = await controller.CreateDocument(cancellationToken);

            //Assert
            actionResult.Should().NotBeNull();
            actionResult.Should().BeOfType<BadRequestObjectResult>();
        }  

        private static MultipartFormDataContent CreateMultiPartFormData(string? fileMimeType = "image/png")
        {
            var streamContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("sample")));
            streamContent.Headers.Add("Content-Type", fileMimeType);
            streamContent.Headers.Add("Content-Disposition", $"form-data; name=\"name\"; filename=\"filename.png\"");

            var nameContent = new StringContent("Name");
            nameContent.Headers.Add("Content-Disposition", $"form-data; name=\"Name\"");
            var descriptionContent = new StringContent("Description");
            descriptionContent.Headers.Add("Content-Disposition", $"form-data; name=\"Description\"");
            var categoryContent = new StringContent("Category");
            categoryContent.Headers.Add("Content-Disposition", $"form-data; name=\"Category\"");

            var multiPartFormData = new MultipartFormDataContent("------")
            {
                nameContent,
                descriptionContent,
                categoryContent,
                streamContent
            };
            return multiPartFormData;
        }
    }
}
