using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using PostgreSQLDocumentManager.Configuration;
using PostgreSQLDocumentManager.Utilities;

namespace PostgreSQLDocumentManager.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion(1.0)]
    public class DocumentsController : ControllerBase
    {
        private readonly AWSConfiguration _AWSConfiguration;
        private readonly ILogger<DocumentsController> logger;

        public DocumentsController(ILogger<DocumentsController> logger, IOptions<AWSConfiguration> AWSConfiguration)
        {
            _AWSConfiguration = AWSConfiguration.Value;
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin,Manager,User")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadObjectAsync(CancellationToken cancellationToken)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File", $"The request couldn't be processed .");
                logger.LogError("Unable to upload file due to invalid request content type.");
                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper
                          .GetBoundary(MediaTypeHeaderValue
                          .Parse(Request.ContentType), FormOptions.DefaultMultipartBoundaryLengthLimit);

            var multipartReader = new MultipartReader(boundary, HttpContext.Request.Body);
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

            var keyName = "Sample";
            var s3Client = new AmazonS3Client(_AWSConfiguration.S3.AccessKeyId, _AWSConfiguration.S3.AccessKeySecret, regionEndpoint);

            // Upload parts.
            var section = await multipartReader.ReadNextSectionAsync(cancellationToken);

            try
            {
                while (section != null)
                {
                    var fileStream = section.AsFileSection()?.FileStream;
                    if (fileStream != null)
                    {
                        var memoryStream = new MemoryStream();
                        await fileStream.CopyToAsync(memoryStream, cancellationToken);

                        var fileTransferUtility = new TransferUtility(s3Client);
                        await fileTransferUtility.UploadAsync(memoryStream, _AWSConfiguration.S3.BucketName, keyName, cancellationToken);
                    }
                    else
                    {
                        var formDataSection = section.AsFormDataSection();
                        if (formDataSection != null)
                        {
                            string? formName = formDataSection.Name;
                            string? value = await formDataSection.Section.ReadAsStringAsync(cancellationToken);
                            logger.LogInformation($"{formName}, {value}");
                        }
                    }

                    section = await multipartReader.ReadNextSectionAsync(cancellationToken);
                }

                return Ok();
            }
            catch (AmazonS3Exception exception)
            {
                logger.LogError(exception, "An AmazonS3Exception was thrown: {message}", exception.Message);
                return Problem("Unable to upload file");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An AmazonS3Exception was thrown: {message}", exception.Message);
                return Problem("Unable to upload file");
            }
        }


        //[HttpPost]
        //[ProducesResponseType(typeof(void), 200)]
        //[ProducesResponseType(typeof(void), 401)]
        //[ProducesResponseType(typeof(void), 403)]
        //[ProducesResponseType(typeof(ProblemDetails), 500)]
        //[Authorize(Roles = "Admin,Manager,User")]
        //[DisableFormValueModelBinding]
        //public async Task<IActionResult> UploadObjectAsync()
        //{
        //    if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        //    {
        //        ModelState.AddModelError("File", $"The request couldn't be processed .");
        //        logger.LogError("Unable to upload file due to invalid request content type.");
        //        return BadRequest(ModelState);
        //    }

        //    var boundary = MultipartRequestHelper
        //                  .GetBoundary(MediaTypeHeaderValue
        //                  .Parse(Request.ContentType), FormOptions.DefaultMultipartBoundaryLengthLimit);

        //    var multipartReader = new MultipartReader(boundary, HttpContext.Request.Body);

        //    RegionEndpoint? regionEndpoint;
        //    try
        //    {
        //        regionEndpoint = RegionEndpoint.GetBySystemName(_AWSConfiguration.S3.Region);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogCritical(ex, "Unable to recover Region Endpoint based on the configuration. Region: {Region}", _AWSConfiguration.S3.Region);
        //        throw;
        //    }

        //    var keyName = "Sample";
        //    var s3Client = new AmazonS3Client(_AWSConfiguration.S3.AccessKeyId, _AWSConfiguration.S3.AccessKeySecret, regionEndpoint);

        //    // Create list to store upload part responses.
        //    List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

        //    // Setup information required to initiate the multipart upload.
        //    InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
        //    {
        //        BucketName = _AWSConfiguration.S3.BucketName,
        //        Key = keyName
        //    };

        //    // Initiate the upload.
        //    InitiateMultipartUploadResponse initResponse = await s3Client.InitiateMultipartUploadAsync(initiateRequest);
        //    // Upload parts.
        //    var section = await multipartReader.ReadNextSectionAsync();

        //    try
        //    {
        //        logger.LogInformation("Uploading parts");

        //        int i = 1;
        //        long filePosition = 0;
        //        while (section != null)
        //        {
        //            var fileStream = section.AsFileSection()?.FileStream;
        //            if (fileStream != null)
        //            {
        //                var memoryStream = new MemoryStream();
        //                await fileStream.CopyToAsync(memoryStream);

        //                var partSize = memoryStream.Length;
        //                UploadPartRequest uploadRequest = new UploadPartRequest
        //                {
        //                    BucketName = _AWSConfiguration.S3.BucketName,
        //                    Key = keyName,
        //                    UploadId = initResponse.UploadId,
        //                    InputStream = memoryStream,
        //                    PartNumber = i,
        //                    PartSize = partSize,
        //                };

        //                uploadRequest.StreamTransferProgress += new EventHandler<StreamTransferProgressArgs>(UploadPartProgressEventCallback);

        //                // Upload a part and add the response to our list.
        //                uploadResponses.Add(await s3Client.UploadPartAsync(uploadRequest));
        //                filePosition += partSize;
        //                i++;
        //            }
        //            else
        //            {
        //                var formDataSection = section.AsFormDataSection();
        //                if (formDataSection != null)
        //                {
        //                    string? formName = formDataSection.Name;
        //                    string? value = await formDataSection.Section.ReadAsStringAsync();
        //                    logger.LogInformation($"{formName}, {value}");
        //                }
        //            }

        //            section = await multipartReader.ReadNextSectionAsync();
        //        }

        //        // Setup to complete the upload.
        //        CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
        //        {
        //            BucketName = _AWSConfiguration.S3.BucketName,
        //            Key = keyName,
        //            UploadId = initResponse.UploadId
        //        };
        //        completeRequest.AddPartETags(uploadResponses);

        //        // Complete the upload.
        //        CompleteMultipartUploadResponse completeUploadResponse = await s3Client.CompleteMultipartUploadAsync(completeRequest);
        //        logger.LogInformation("File Upload completed. Key: {key}, Content Length: {Length}", completeUploadResponse.Key, completeUploadResponse.ContentLength);

        //        return Ok();
        //    }
        //    catch (Exception exception)
        //    {
        //        logger.LogError(exception, "An AmazonS3Exception was thrown: {0}", exception.Message);

        //        // Abort the upload.
        //        AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
        //        {
        //            BucketName = _AWSConfiguration.S3.BucketName,
        //            Key = keyName,
        //            UploadId = initResponse.UploadId
        //        };
        //        await s3Client.AbortMultipartUploadAsync(abortMPURequest);

        //        return Problem("Unable to upload file");
        //    }
        //}

        //private void UploadPartProgressEventCallback(object? sender, StreamTransferProgressArgs e)
        //{
        //    logger.LogDebug("{transferedBytes}/{totalBytes}", e.TransferredBytes, e.TotalBytes);
        //}
    }
}
