using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
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

        public DocumentService(ILogger<DocumentService> logger, IFileRepository fileRepository, IDocumentRepository documentRepository)
        {
            this.logger = logger;
            this.fileRepository = fileRepository;
            this.documentRepository = documentRepository;
        }

        public async Task<DownloadDocumentResponse> DownloadFileAsync(int id, CancellationToken cancellationToken = default)
        {
            var existingDocument = await documentRepository.GetAsync(id, cancellationToken);
            //Todo: check if document was properly uploaded
            if (existingDocument == null || !existingDocument.Uploaded)
            {
                throw new ServiceException($"File was not found or not properly uploaded.");
            }

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
                if (existingDocument != null && existingDocument.Uploaded)
                {
                    throw new ServiceException($"Unable to create document. Document with name: {fileUploadRequest.Name}," +
                        $" category: {fileUploadRequest.Category}, description: {fileUploadRequest.Description} already exists.");
                }

                logger.LogInformation("Adding document info into database");
                var createdDocument = await documentRepository.AddAsync(fileUploadRequest.Name!, fileUploadRequest.Description!, fileUploadRequest.Category!, fileUploadRequest.GetKeyName(), cancellationToken: cancellationToken);

                logger.LogInformation("Uploading file into file repository");
                var uploadSuccess = await fileRepository.UploadFromStreamAsync(fileStream, contentType, fileUploadRequest.GetKeyName(),
                    fileUploadRequest.Name!, fileUploadRequest.Category!, fileUploadRequest.Description!, cancellationToken);

                if (!uploadSuccess)
                {
                    throw new ServiceException("Unable to upload file to storage.");
                }

                logger.LogInformation("Updating file uploaded information");
                var udpatedDocument = await documentRepository.UpdateUploadedStatusAsync(createdDocument.Id, uploadSuccess, cancellationToken);
                if (udpatedDocument == null)
                {
                    throw new ServiceException("File was created and uploaded, uploaded flag was not set.");                    
                }

                return new CreateDocumentResponse(udpatedDocument);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Unexpected issue while attempting upload file. Message: {ex.Message}", ex);
            }

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
    }
}
