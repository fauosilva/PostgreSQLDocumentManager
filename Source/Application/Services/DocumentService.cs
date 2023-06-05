using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ILogger<DocumentService> logger;
        private readonly IFileRepository fileRepository;
        private readonly IDocumentRepository documentRepository;
        private readonly IDocumentPermissionRepository documentPermissionRepository;

        public DocumentService(ILogger<DocumentService> logger, IFileRepository fileRepository, IDocumentRepository documentRepository,
            IDocumentPermissionRepository documentPermissionRepository)
        {
            this.logger = logger;
            this.fileRepository = fileRepository;
            this.documentRepository = documentRepository;
            this.documentPermissionRepository = documentPermissionRepository;
        }

        public async Task<DownloadDocumentResponse> DownloadFileAsync(int id, CancellationToken cancellationToken = default)
        {
            Document existingDocument = await RetrieveExistingDocumentAsync(id, cancellationToken);
            var downloadResult = await fileRepository.DownloadFileAsync(existingDocument.KeyName, cancellationToken);
            return new DownloadDocumentResponse(downloadResult.Item1, existingDocument.Name, downloadResult.Item2);
        }

        public async Task<CreateDocumentResponse> UploadFileAsync(Stream fileStream, string? contentType, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default)
        {
            var uploadRequestValidation = fileUploadRequest.Validate(new ValidationContext(fileUploadRequest));
            if (uploadRequestValidation.Any())
            {
                throw new ServiceException($"Unable to upload file to storage. Invalid metadata. {string.Join(',', uploadRequestValidation)}");
            }

            try
            {
                var existingDocument = await documentRepository.GetDocumentByNameDescriptionAndCategoryAsync(fileUploadRequest.Name!, fileUploadRequest.Description!, fileUploadRequest.Category!, cancellationToken);
                if (existingDocument != null)
                {
                    if (existingDocument.Uploaded)
                    {
                        throw new ServiceException($"Unable to create document. Document with name: {fileUploadRequest.Name}," +
                            $" category: {fileUploadRequest.Category}, description: {fileUploadRequest.Description} already exists.");
                    }
                    else
                    {
                        logger.LogInformation("File already on the database but upload flag is not set.");
                        return await InternalUpdloadFileAsync(fileStream, contentType, fileUploadRequest, existingDocument, cancellationToken);
                    }
                }

                logger.LogInformation("Adding document info into database");
                var createdDocument = await documentRepository.AddAsync(fileUploadRequest.Name!, fileUploadRequest.Description!, fileUploadRequest.Category!, fileUploadRequest.GetKeyName(), cancellationToken: cancellationToken);

                return await InternalUpdloadFileAsync(fileStream, contentType, fileUploadRequest, createdDocument, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Unexpected issue while attempting upload file. Message: {ex.Message}", ex);
            }

        }

        private async Task<CreateDocumentResponse> InternalUpdloadFileAsync(Stream fileStream, string? contentType, FileUploadRequest fileUploadRequest, Document createdDocument, CancellationToken cancellationToken)
        {
            logger.LogInformation("Uploading file into file repository");
            var uploadSuccess = await fileRepository.UploadFromStreamAsync(fileStream, contentType, fileUploadRequest.GetKeyName(),
                fileUploadRequest.Name!, fileUploadRequest.Category!, fileUploadRequest.Description!, cancellationToken);

            if (!uploadSuccess)
            {
                throw new ServiceException("Unable to upload file to storage.");
            }

            logger.LogInformation("Updating file uploaded information");
            var udpatedDocument = await documentRepository.UpdateUploadedStatusAsync(createdDocument.Id, uploadSuccess, cancellationToken)
                ?? throw new ServiceException("File was created and uploaded, uploaded flag was not set.");

            return new CreateDocumentResponse(udpatedDocument);
        }

        public async Task<IEnumerable<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var documents = await documentRepository.GetAllAsync(cancellationToken);
            if (documents != null && documents.Any())
            {
                return documents.Select(document => new DocumentResponse(document));
            }

            return new List<DocumentResponse>();
        }

        public async Task<CreatePermissionResponse> CreateUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            var documentPermission = await InternalPermissionExecutorAsync(id, userId,
                async (id, groupId) => await documentPermissionRepository.AddUserPermissionAsync(id, groupId, cancellationToken), cancellationToken);
            return new CreatePermissionResponse(documentPermission);
        }

        public async Task<CreatePermissionResponse> CreateGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default)
        {
            var documentPermission = await InternalPermissionExecutorAsync(id, groupId,
                async (id, groupId) => await documentPermissionRepository.AddGroupPermissionAsync(id, groupId, cancellationToken), cancellationToken);
            return new CreatePermissionResponse(documentPermission);
        }

        public async Task<bool> DeleteUserPermissionAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            return await InternalPermissionExecutorAsync(id, userId,
                async (id, userId) => await documentPermissionRepository.DeleteUserPermissionAsync(id, userId, cancellationToken), cancellationToken);
        }

        public async Task<bool> DeleteGroupPermissionAsync(int id, int groupId, CancellationToken cancellationToken = default)
        {
            return await InternalPermissionExecutorAsync(id, groupId,
                 async (id, groupId) => await documentPermissionRepository.DeleteGroupPermissionAsync(id, groupId, cancellationToken), cancellationToken);
        }

        private async Task<T> InternalPermissionExecutorAsync<T>(int documentId, int relatedEntityId, Func<int, int, Task<T>> func, CancellationToken cancellationToken = default)
        {
            _ = await RetrieveExistingDocumentAsync(documentId, cancellationToken);
            return await func(documentId, relatedEntityId);
        }

        private async Task<Document> RetrieveExistingDocumentAsync(int id, CancellationToken cancellationToken)
        {
            var existingDocument = await documentRepository.GetAsync(id, cancellationToken);
            if (existingDocument == null || !existingDocument.Uploaded)
            {
                throw new ServiceException($"File was not found or not properly uploaded.");
            }

            return existingDocument;
        }
    }
}
