using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using ApplicationCore.Dtos.Requests;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Files
{
    public class FileRepository : IFileRepository
    {
        private readonly ILogger<FileRepository> logger;
        private readonly AWSConfiguration _AWSConfiguration;

        public FileRepository(ILogger<FileRepository> logger, IOptions<AWSConfiguration> AWSConfiguration)
        {
            this.logger = logger;
            _AWSConfiguration = AWSConfiguration.Value;
        }

        public async Task<bool> UploadFileAsync(Stream fileStream, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken = default)
        {
            if (!fileUploadRequest.IsValid())
            {
                return false;
            }

            RegionEndpoint? regionEndpoint;
            try
            {
                regionEndpoint = RegionEndpoint.GetBySystemName(_AWSConfiguration.S3.Region);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unable to recover Region Endpoint based on the configuration. Region: {Region}", _AWSConfiguration.S3.Region);
                throw;
            }

            try
            {
                if (fileStream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                        {
                            BucketName = _AWSConfiguration.S3.BucketName,
                            InputStream = memoryStream,
                            StorageClass = S3StorageClass.StandardInfrequentAccess,
                            Key = fileUploadRequest.GetKeyName()
                        };

                        //Todo: Add Content-Type from the file extensions.                            
                        fileTransferUtilityRequest.Metadata.Add(nameof(fileUploadRequest.Name), fileUploadRequest.Name);
                        fileTransferUtilityRequest.Metadata.Add(nameof(fileUploadRequest.Category), fileUploadRequest.Category);
                        fileTransferUtilityRequest.Metadata.Add(nameof(fileUploadRequest.Description), fileUploadRequest.Description);

                        await fileStream.CopyToAsync(memoryStream, cancellationToken);

                        var s3Client = new AmazonS3Client(_AWSConfiguration.S3.AccessKeyId, _AWSConfiguration.S3.AccessKeySecret, regionEndpoint);
                        var fileTransferUtility = new TransferUtility(s3Client);
                        await fileTransferUtility.UploadAsync(fileTransferUtilityRequest, cancellationToken);
                    }
                }
                return true;
            }
            catch (AmazonS3Exception exception)
            {
                logger.LogError(exception, "An AmazonS3Exception was thrown: {message}", exception.Message);
                return false;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An exception was thrown: {message}", exception.Message);
                return false;
            }
        }
    }
}
