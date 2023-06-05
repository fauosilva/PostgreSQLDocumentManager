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
    public class DocumentServiceTests
    {
        private readonly ILogger<DocumentService> logger;

        public DocumentServiceTests()
        {
            logger = NullLoggerFactory.Instance.CreateLogger<DocumentService>();
        }

        [Fact]
        public async Task GetDocumentAsync_ShouldReturnNull_WhenGetDocumentAsyncReturnsNull()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            documentRepository.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => null);
            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act            
            var result = await documentService.GetDocumentsAsync(cancellationToken);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDocumentAsync_ShouldDocumentResponseList_WhenGetAllAsyncReturnsRecords()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();
            var response = new List<Document>()
            {
                new Document() { Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample", Category = "Category", Description = "Description", Uploaded = true, KeyName = "KeyName" },
                new Document() { Id = 2, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", Name = "sample", Category = "Category", Description = "Description", Uploaded = true, KeyName = "KeyName" }
            };
            documentRepository.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act            
            var result = await documentService.GetDocumentsAsync(cancellationToken);

            //Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(response, options =>
                options.Excluding(f => f.Permissions)
                .WithAuditableMapping());
        }

        [Fact]
        public async Task UploadFileAsync_ShouldReturnServiceException_WhenErrorHappensInGetByDocumentAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            documentRepository.Setup(m => m.GetDocumentByNameDescriptionAndCategoryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Sample"));

            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);
            var createDocumentRequest = new FileUploadRequest();
            createDocumentRequest.AddMetadata("Name", "Name");
            createDocumentRequest.AddMetadata("Category", "Category");
            createDocumentRequest.AddMetadata("Description", "Description");

            //Act
            Exception? returnedException = null;
            try
            {
                await documentService.UploadFileAsync(new MemoryStream(), "image/png", createDocumentRequest, cancellationToken);
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
        public async Task CreateDocumentAsync_ShouldReturnServiceException_WhenExistingDocumentIsReturnedByGetByDocumentnameAsync()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            documentRepository.Setup(m => m.GetDocumentByNameDescriptionAndCategoryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Document() { Uploaded = true });

            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);
            var createDocumentRequest = new FileUploadRequest();
            createDocumentRequest.AddMetadata("Name", "Name");
            createDocumentRequest.AddMetadata("Category", "Category");
            createDocumentRequest.AddMetadata("Description", "Description");

            //Act
            Exception? returnedException = null;
            try
            {
                await documentService.UploadFileAsync(new MemoryStream(), "image/png", createDocumentRequest, cancellationToken);
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
        public async Task CreateDocumentAsync_ShouldReturnServiceException_WhenUploadFails()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            var createDocumentRequest = new FileUploadRequest();
            createDocumentRequest.AddMetadata("Name", "Name");
            createDocumentRequest.AddMetadata("Category", "Category");
            createDocumentRequest.AddMetadata("Description", "Description");

            documentRepository.Setup(m => m.GetDocumentByNameDescriptionAndCategoryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            fileRepository.Setup(m => m.UploadFromStreamAsync(new MemoryStream(), "image/png", createDocumentRequest.GetKeyName(),
                createDocumentRequest.Name!, createDocumentRequest.Category!, createDocumentRequest.Description!, cancellationToken))
                .ReturnsAsync(false);

            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            try
            {
                await documentService.UploadFileAsync(new MemoryStream(), "image/png", createDocumentRequest, cancellationToken);
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
        public async Task CreateDocumentAsync_ShouldReturnCreatedDocument_WhenUploadIsCompleted()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            var createDocumentRequest = new FileUploadRequest();
            createDocumentRequest.AddMetadata("Name", "Name");
            createDocumentRequest.AddMetadata("Category", "Category");
            createDocumentRequest.AddMetadata("Description", "Description");

            documentRepository.Setup(m => m.GetDocumentByNameDescriptionAndCategoryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            documentRepository.Setup(m => m.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Document()
                {
                    Id = 1,
                    Category = "Category",
                    Description = "Description",
                    Name = "Name",
                    Uploaded = false,
                    Inserted_At = DateTime.UtcNow,
                    Inserted_By = "Sample",
                    KeyName = "SampleKey"
                });

            var response = new Document()
            {
                Id = 1,
                Category = "Category",
                Description = "Description",
                Name = "Name",
                Uploaded = true,
                Inserted_At = DateTime.UtcNow,
                Inserted_By = "Sample",
                KeyName = "SampleKey",
                Updated_At = DateTime.UtcNow,
                Updated_By = "Sample"
            };

            documentRepository.Setup(m => m.UpdateUploadedStatusAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => response);

            fileRepository.Setup(m => m.UploadFromStreamAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            CreateDocumentResponse? result = null;
            try
            {
                result = await documentService.UploadFileAsync(new MemoryStream(), "image/png", createDocumentRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(response, options =>
                options.Excluding(f => f.Permissions)
                .WithAuditableMapping());
        }

        [Fact]
        public async Task CreateDocumentAsync_ShouldReturnCreatedDocument_WhenUploadIsOnTheDatabaseButNotCompleted()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            var createDocumentRequest = new FileUploadRequest();
            createDocumentRequest.AddMetadata("Name", "Name");
            createDocumentRequest.AddMetadata("Category", "Category");
            createDocumentRequest.AddMetadata("Description", "Description");

            documentRepository.Setup(m => m.GetDocumentByNameDescriptionAndCategoryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Document()
                {
                    Id = 1,
                    Category = "Category",
                    Description = "Description",
                    Name = "Name",
                    Uploaded = false,
                    Inserted_At = DateTime.UtcNow,
                    Inserted_By = "Sample",
                    KeyName = "SampleKey"
                });

            fileRepository.Setup(m => m.UploadFromStreamAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
             It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

            var response = new Document()
            {
                Id = 1,
                Category = "Category",
                Description = "Description",
                Name = "Name",
                Uploaded = true,
                Inserted_At = DateTime.UtcNow,
                Inserted_By = "Sample",
                KeyName = "SampleKey",
                Updated_At = DateTime.UtcNow,
                Updated_By = "Sample"
            };

            documentRepository.Setup(m => m.UpdateUploadedStatusAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => response);

            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            CreateDocumentResponse? result = null;
            try
            {
                result = await documentService.UploadFileAsync(new MemoryStream(), "image/png", createDocumentRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                returnedException = ex;
            }

            //Assert
            returnedException.Should().BeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(response, options =>
                options.Excluding(f => f.Permissions)
                .WithAuditableMapping());
        }

        [Fact]
        public async Task CreateUserPermission_ShouldReturnCreatedPermission_WhenUserPermissionIsCreatedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            var response = new DocumentPermission() { Document_Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", User_Id = 1 };
            documentRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Document() { Uploaded = true });
            documentPermissionRepository.Setup(m => m.AddUserPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            CreatePermissionResponse? result = null;
            try
            {
                result = await documentService.CreateUserPermissionAsync(1, 1, cancellationToken);
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
                .Excluding(m => m.Group_Id)
                .WithMapping<CreatePermissionResponse>(m => m.Document_Id, s => s.DocumentId)
                .WithMapping<CreatePermissionResponse>(m => m.User_Id, s => s.UserId));
        }

        [Fact]
        public async Task CreateGroupPermission_ShouldReturnCreatedPermission_WhenGroupPermissionIsCreatedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            var response = new DocumentPermission() { Document_Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", User_Id = 1 };
            documentRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Document() { Uploaded = true });
            documentPermissionRepository.Setup(m => m.AddGroupPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            CreatePermissionResponse? result = null;
            try
            {
                result = await documentService.CreateGroupPermissionAsync(1, 1, cancellationToken);
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
                .Excluding(m => m.Group_Id)
                .WithMapping<CreatePermissionResponse>(m => m.Document_Id, s => s.DocumentId)
                .WithMapping<CreatePermissionResponse>(m => m.User_Id, s => s.UserId));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public async Task CreateUserPermission_ShouldReturnServiceException_WhenDocumentDoesNotExist(bool? uploaded)
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            var response = new DocumentPermission() { Document_Id = 1, Inserted_At = DateTime.UtcNow, Inserted_By = "sample", User_Id = 1 };
            Document? document;
            if (uploaded == null)
            {
                document = null;
            }
            else
            {
                document = new Document() { Uploaded = false };
            }

            documentRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => document);
            documentPermissionRepository.Setup(m => m.AddUserPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            CreatePermissionResponse? result = null;
            try
            {
                result = await documentService.CreateUserPermissionAsync(1, 1, cancellationToken);
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
        public async Task DeleteUserPermissionAsync_ShouldReturnTrue_WhenUserPermissionIsDeletedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            documentRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Document() { Uploaded = true });
            documentPermissionRepository.Setup(m => m.DeleteUserPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            bool? result = null;
            try
            {
                result = await documentService.DeleteUserPermissionAsync(1, 1, cancellationToken);
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
        public async Task DeleteGroupPermissionAsync_ShouldReturnTrue_WhenGroupPermissionIsDeletedWithSuccess()
        {
            //Arrange
            var cancellationToken = CancellationToken.None;
            var fileRepository = new Mock<IFileRepository>();
            var documentRepository = new Mock<IDocumentRepository>();
            var documentPermissionRepository = new Mock<IDocumentPermissionRepository>();

            documentRepository.Setup(m => m.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Document() { Uploaded = true });
            documentPermissionRepository.Setup(m => m.DeleteGroupPermissionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var documentService = new DocumentService(logger, fileRepository.Object, documentRepository.Object, documentPermissionRepository.Object);

            //Act
            Exception? returnedException = null;
            bool? result = null;
            try
            {
                result = await documentService.DeleteGroupPermissionAsync(1, 1, cancellationToken);
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
    }
}
