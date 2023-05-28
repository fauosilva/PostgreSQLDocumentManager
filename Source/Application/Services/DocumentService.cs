using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IFileRepository fileRepository;
        private readonly IDocumentRepository documentRepository;

        public DocumentService(IFileRepository fileRepository, IDocumentRepository documentRepository)
        {
            this.fileRepository = fileRepository;
            this.documentRepository = documentRepository;
        }

        public async Task<Stream> DownloadFileAsync(string keyName, CancellationToken cancellationToken = default)
        {
            return await fileRepository.DownloadFileAsync(keyName, cancellationToken);
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

                var createdDocument = await documentRepository.AddAsync(fileUploadRequest.Name!, fileUploadRequest.Description!, fileUploadRequest.Category!, fileUploadRequest.GetKeyName(), cancellationToken: cancellationToken);

                var uploadSuccess = await fileRepository.UploadFromStreamAsync(fileStream, contentType, fileUploadRequest.GetKeyName(),
                    fileUploadRequest.Name!, fileUploadRequest.Category!, fileUploadRequest.Description!, cancellationToken);

                if (!uploadSuccess)
                {
                    throw new ServiceException("Unable to upload file to storage.");
                }

                //Todo: Update uploaded collumn
                return new CreateDocumentResponse(createdDocument);

            }
            catch (Exception ex)
            {
                throw new ServiceException($"Unexpected issue while attempting upload file. Message: {ex.Message}", ex);
            }

        }

        public async Task<DocumentResponse?> GetDocumentAsync(int id, CancellationToken cancellationToken = default)
        {
            var document = await documentRepository.GetAsync(id, cancellationToken);
            if (document != null)
            {
                return new DocumentResponse(document);
            }

            return default;
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
