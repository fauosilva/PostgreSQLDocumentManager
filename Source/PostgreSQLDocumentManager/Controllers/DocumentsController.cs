using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using PostgreSQLDocumentManager.Utilities;

namespace PostgreSQLDocumentManager.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion(1.0)]
    public class DocumentsController : ControllerBase
    {
        private readonly ILogger<DocumentsController> logger;
        private readonly IFileService fileService;

        public DocumentsController(ILogger<DocumentsController> logger, IFileService fileService)
        {
            this.logger = logger;
            this.fileService = fileService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin,Manager")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadObjectAsync(CancellationToken cancellationToken)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                logger.LogError("Unable to upload file due to invalid request content type.");

                ModelState.AddModelError("File", $"The request couldn't be processed due to invalid content type.");
                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper
                          .GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), FormOptions.DefaultMultipartBoundaryLengthLimit);

            var multipartReader = new MultipartReader(boundary, HttpContext.Request.Body);

            var fileUploadRequest = new FileUploadRequest();
            FileUploadResponse? fileUploadResponse = null;

            // Upload parts.
            var section = await multipartReader.ReadNextSectionAsync(cancellationToken);

            try
            {
                while (section != null)
                {
                    var formDataSection = section.AsFormDataSection();
                    if (formDataSection != null)
                    {
                        await HandleFormData(fileUploadRequest, formDataSection, cancellationToken);
                    }
                    else
                    {
                        fileUploadResponse = await HandleFileUpload(fileUploadRequest, section, cancellationToken);
                    }
                    section = await multipartReader.ReadNextSectionAsync(cancellationToken);
                }

                if (fileUploadResponse != null)
                    return Ok(fileUploadResponse);

                return Problem("Unable to upload file and return file information");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An exception was thrown: {message}", exception.Message);
                return Problem("Unable to upload file.");
            }
        }

        private async Task<FileUploadResponse?> HandleFileUpload(FileUploadRequest fileUploadRequest, MultipartSection section, CancellationToken cancellationToken)
        {
            var fileStream = section.AsFileSection()?.FileStream;
            var contentType = section.Headers?.GetValueOrDefault(HeaderNames.ContentType);

            if (fileStream != null)
            {
                logger.LogDebug("Starting file upload service");
                return await fileService.UploadFileAsync(fileStream, contentType, fileUploadRequest, cancellationToken);
            }

            return null;
        }

        private async Task HandleFormData(FileUploadRequest fileUploadRequest, FormMultipartSection formDataSection, CancellationToken cancellationToken)
        {
            string sectionName = formDataSection.Name;
            string value = await formDataSection.Section.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("File metadata received {sectionName}, {value}.", sectionName, value);

            fileUploadRequest.AddMetadata(sectionName, value);
        }
    }
}
