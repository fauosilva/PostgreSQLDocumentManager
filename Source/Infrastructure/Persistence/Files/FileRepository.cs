using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;

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

        public async Task<bool> UploadFileAsync(Stream fileStream, string? contentType, string key, string name, string category, string description, CancellationToken cancellationToken = default)
        {
            RegionEndpoint? regionEndpoint = GetRegionEndpoint();
            try
            {
                if (fileStream != null)
                {
                    using var memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream, cancellationToken);

                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = _AWSConfiguration.S3.BucketName,
                        InputStream = memoryStream,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        Key = key
                    };

                    if (!string.IsNullOrEmpty(contentType))
                    {
                        fileTransferUtilityRequest.ContentType = contentType;
                    }
                    fileTransferUtilityRequest.Metadata.Add(nameof(name), name);
                    fileTransferUtilityRequest.Metadata.Add(nameof(category), category);
                    fileTransferUtilityRequest.Metadata.Add(nameof(description), description);

                    var s3Client = new AmazonS3Client(_AWSConfiguration.S3.AccessKeyId, _AWSConfiguration.S3.AccessKeySecret, regionEndpoint);
                    var fileTransferUtility = new TransferUtility(s3Client);

                    await fileTransferUtility.UploadAsync(fileStream, _AWSConfiguration.S3.BucketName, key, cancellationToken);
                    return true;
                }

                return false;
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

        private RegionEndpoint GetRegionEndpoint()
        {
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

            return regionEndpoint;
        }

        /// <summary>
        /// Upload for files larger than 5MB (5242880 bits) and unknown max size.
        /// </summary>
        /// <param name="fileStream">Filestream that contains the file content</param>
        /// <param name="contentType">File content type</param>
        /// <param name="key">File unique identifier</param>
        /// <param name="name">File name</param>
        /// <param name="category">File category</param>
        /// <param name="description">File descrpition</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<bool> UploadLargeFileAsync(Stream fileStream, string? contentType, string key, string name, string category, string description, CancellationToken cancellationToken = default)
        {
            RegionEndpoint? regionEndpoint = GetRegionEndpoint();
            List<UploadPartResponse> uploadResponses = new();

            var s3Client = new AmazonS3Client(_AWSConfiguration.S3.AccessKeyId, _AWSConfiguration.S3.AccessKeySecret, regionEndpoint);

            // Setup information required to initiate the multipart upload.
            InitiateMultipartUploadRequest initiateRequest = new()
            {
                BucketName = _AWSConfiguration.S3.BucketName,
                Key = key
            };

            if (!string.IsNullOrEmpty(contentType))
            {
                initiateRequest.ContentType = contentType;
            }
            initiateRequest.Metadata.Add(nameof(name), name);
            initiateRequest.Metadata.Add(nameof(category), category);
            initiateRequest.Metadata.Add(nameof(description), description);

            // Initiate the upload.
            InitiateMultipartUploadResponse initResponse = await s3Client.InitiateMultipartUploadAsync(initiateRequest, cancellationToken);

            try
            {
                logger.LogInformation("Upload large file using parts");

                int partNumber = 1;
                int partSize = 5242880; // 5 MB

                byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
                using var memoryStream = new MemoryStream();
                try
                {
                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false)) != 0)
                    {
                        await memoryStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                        if (memoryStream.Length >= partSize)
                        {
                            await UploadPartAsync(key, uploadResponses, s3Client, initResponse, partNumber, memoryStream, false, cancellationToken);
                            memoryStream.SetLength(0);
                            partNumber++;
                        }
                    }
                }
                finally
                {
                    await UploadPartAsync(key, uploadResponses, s3Client, initResponse, partNumber, memoryStream, true, cancellationToken);
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                // Setup to complete the upload.
                CompleteMultipartUploadRequest completeRequest = new()
                {
                    BucketName = _AWSConfiguration.S3.BucketName,
                    Key = key,
                    UploadId = initResponse.UploadId,
                };
                completeRequest.AddPartETags(uploadResponses);

                // Complete the upload.
                CompleteMultipartUploadResponse completeUploadResponse = await s3Client.CompleteMultipartUploadAsync(completeRequest, cancellationToken);
                logger.LogDebug("Multipart upload completed.Key: {key}, StatusCode: {statusCode}", completeUploadResponse.Key, completeUploadResponse.HttpStatusCode);
                return completeUploadResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An exception was thrown: {message}", exception.Message);

                // Abort the upload.
                AbortMultipartUploadRequest abortMPURequest = new()
                {
                    BucketName = _AWSConfiguration.S3.BucketName,
                    Key = key,
                    UploadId = initResponse.UploadId
                };

                logger.LogWarning("Attempting to abort multipart upload");
                var response = await s3Client.AbortMultipartUploadAsync(abortMPURequest, cancellationToken);
                logger.LogWarning("Abort multi part upload response {statusCode}, {stringResult}", response.HttpStatusCode, response.ToString());

                return false;
            }
        }

        private async Task UploadPartAsync(string key, List<UploadPartResponse> uploadResponses, AmazonS3Client s3Client, InitiateMultipartUploadResponse initResponse, int partNumber, MemoryStream memoryStream, bool isLastPart, CancellationToken cancellationToken)
        {
            using var completeStream = new MemoryStream(memoryStream.ToArray());

            logger.LogWarning("UploadPartRequest for partNumber {partNumber} is going to be created with {memoryStream.Length} Kb", partNumber, completeStream.Length / 1024);
            UploadPartRequest uploadRequest = new()
            {
                BucketName = _AWSConfiguration.S3.BucketName,
                Key = key,
                UploadId = initResponse.UploadId,
                PartNumber = partNumber,
                PartSize = completeStream.Length,
                InputStream = completeStream,
                IsLastPart = isLastPart
            };

            // Upload a part and add the response to our list.
            var uploadResponse = await s3Client.UploadPartAsync(uploadRequest, cancellationToken);
            uploadResponses.Add(uploadResponse);
        }
    }
}


