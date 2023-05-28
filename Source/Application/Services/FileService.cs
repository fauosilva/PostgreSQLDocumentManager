using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository fileRepository;
        private readonly IDocumentRepository documentRepository;

        public FileService(IFileRepository fileRepository, IDocumentRepository documentRepository)
        {
            this.fileRepository = fileRepository;
            this.documentRepository = documentRepository;
        }

        public async Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string? contentType, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default)
        {
            var uploadRequestValidation = fileUploadRequest.Validate(new ValidationContext(fileUploadRequest));
            if (uploadRequestValidation.Any())
            {
                throw new ServiceException($"Unable to upload file to storage. Invalid metadata. {string.Join(',', uploadRequestValidation)}");
            }

            try
            {
                var createdDocument = await documentRepository.AddAsync(fileUploadRequest.Name!, fileUploadRequest.Description!, fileUploadRequest.Category!, fileUploadRequest.GetKeyName(), cancellationToken);

                var uploadSuccess = await fileRepository.UploadLargeFileAsync(fileStream, contentType, fileUploadRequest.GetKeyName(),
                    fileUploadRequest.Name!, fileUploadRequest.Category!, fileUploadRequest.Description!, cancellationToken);

                if (!uploadSuccess)
                {
                    throw new ServiceException("Unable to upload file to storage.");
                }

                return new FileUploadResponse(createdDocument);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Unexpected issue while attempting upload file. Message: {ex.Message}", ex);
            }

        }
    }
}
