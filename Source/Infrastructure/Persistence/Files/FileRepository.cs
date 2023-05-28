using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;

namespace Infrastructure.Persistence.Files
{
    public class FileRepository : IFileRepository
    {
        private readonly ILogger<FileRepository> logger;
        private readonly AmazonS3Client s3Client;
        private readonly string bucketName;

        public FileRepository(ILogger<FileRepository> logger, IOptions<AWSConfiguration> AWSConfiguration)
        {
            this.logger = logger;            
            bucketName = AWSConfiguration.Value.S3.BucketName;
            s3Client = new AmazonS3Client(AWSConfiguration.Value.S3.AccessKeyId, AWSConfiguration.Value.S3.AccessKeySecret,
                GetRegionEndpoint(AWSConfiguration.Value.S3.Region));
        }

        public async Task<(Stream, string)> DownloadFileAsync(string key, CancellationToken cancellationToken = default)
        {
            GetObjectRequest request = new()
            {
                BucketName = bucketName,
                Key = key
            };

            GetObjectResponse? response = null;
            try
            {
                //Not disposing object since they will be used on the service response
                response = await s3Client.GetObjectAsync(request, cancellationToken);
                return (response.ResponseStream, response.Headers.ContentType);
            }
            catch (AmazonS3Exception e)
            {
                response?.Dispose();
                // If bucket or object does not exist
                logger.LogError("Error encountered. Message:'{message}' when reading object", e.Message);
                throw;
            }
            catch (Exception e)
            {
                response?.Dispose();
                logger.LogError("Unknown encountered on server. Message:'{message}' when reading object", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Upload for files with unknown size. 
        /// Files larger than 5MB (5242880 bits) will be split into parts to be uploaded.
        /// </summary>
        /// <param name="fileStream">Filestream that contains the file content</param>
        /// <param name="contentType">File content type</param>
        /// <param name="key">File unique identifier</param>
        /// <param name="name">File name</param>
        /// <param name="category">File category</param>
        /// <param name="description">File descrpition</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True in case of successfull upload, otherwise false.</returns>
        public async Task<bool> UploadFromStreamAsync(Stream fileStream, string? contentType, string key, string name, string category, string description, CancellationToken cancellationToken = default)
        {
            List<UploadPartResponse> uploadResponses = new();

            // Setup information required to initiate the multipart upload.
            InitiateMultipartUploadRequest initiateRequest = new()
            {
                BucketName = bucketName,
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
                int partNumber = 1;
                int partSize = 5242880; // 5 MB
                int bufferLength = 4096; //4kb

                logger.LogDebug("Upload file using parts with partsize {partSize}", partSize);
                
                byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
                using var memoryStream = new MemoryStream();
                try
                {
                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false)) != 0)
                    {                        
                        await memoryStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                        if (memoryStream.Length >= partSize)
                        {
                            logger.LogInformation("File {key} split into parts to upload. PartNumber: {partNumber}", key, partNumber);
                            await UploadPartAsync(key, uploadResponses, s3Client, initResponse, partNumber, memoryStream, false, cancellationToken);
                            
                            //Reusing the same memory stream to accumulate upload parts.
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
                    BucketName = bucketName,
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
                    BucketName = bucketName,
                    Key = key,
                    UploadId = initResponse.UploadId
                };

                logger.LogWarning("Attempting to abort multipart upload");
                var response = await s3Client.AbortMultipartUploadAsync(abortMPURequest, cancellationToken);
                logger.LogWarning("Abort multi part upload response {statusCode}, {stringResult}", response.HttpStatusCode, response.ToString());

                return false;
            }
        }

        private RegionEndpoint GetRegionEndpoint(string region)
        {
            try
            {
                return RegionEndpoint.GetBySystemName(region);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unable to recover Region Endpoint based on the configuration. Region: {Region}", region);
                throw;
            }
        }

        private async Task UploadPartAsync(string key, List<UploadPartResponse> uploadResponses, AmazonS3Client s3Client, InitiateMultipartUploadResponse initResponse, int partNumber, MemoryStream memoryStream, bool isLastPart, CancellationToken cancellationToken)
        {
            using var completeStream = new MemoryStream(memoryStream.ToArray());

            logger.LogInformation("UploadPartRequest for partNumber {partNumber} is going to be created with {memoryStream.Length} Kb", partNumber, completeStream.Length / 1024);
            UploadPartRequest uploadRequest = new()
            {
                BucketName = bucketName,
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


