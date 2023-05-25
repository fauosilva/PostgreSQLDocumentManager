using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;

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

        public async Task<FileUploadResponse> UploadFileAsync(Stream fileStream, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default)
        {
            if (!fileUploadRequest.IsValid())
            {
                throw new ServiceException("Unable to upload file to storage. Invalid metadata");
            }

            var uploadSuccess = await fileRepository.UploadFileAsync(fileStream, fileUploadRequest, cancellationToken);
            if (!uploadSuccess)
            {
                throw new ServiceException("Unable to upload file to storage.");
            }

            var createdDocument = await documentRepository.AddAsync(fileUploadRequest.Name!, fileUploadRequest.Description!, fileUploadRequest.Category!, fileUploadRequest.GetKeyName(), cancellationToken);

            return new FileUploadResponse(createdDocument);
        }
    }
}
