using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
        private readonly ILogger<DocumentsController> logger;
        private readonly IDocumentService documentService;
        private readonly IOptions<FileUploadConfiguration> options;

        public DocumentsController(ILogger<DocumentsController> logger, IDocumentService documentService, IOptions<FileUploadConfiguration> options)
        {
            this.logger = logger;
            this.documentService = documentService;
            this.options = options;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentResponse>), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDocuments(CancellationToken cancellationToken)
        {
            var documents = await documentService.GetDocumentsAsync(cancellationToken);
            return Ok(documents);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentResponse), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadDocument(int id, CancellationToken cancellationToken)
        {
            var document = await documentService.DownloadFileAsync(id, cancellationToken);

            if (document == null)
                return NotFound();

            return File(document.Filestream, document.ContentType, document.Name);
        }

        [HttpPost]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin,Manager")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> CreateDocument(CancellationToken cancellationToken)
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
            CreateDocumentResponse? fileUploadResponse = null;

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
                        var fileStream = section.AsFileSection()?.FileStream;
                        var headerContentType = section.Headers?.GetValueOrDefault(HeaderNames.ContentType).FirstOrDefault();

                        var objectResult = ValidateContentDisposition(section, headerContentType);
                        if (objectResult != null)
                            return objectResult;

                        fileUploadResponse = await HandleFileUpload(fileStream, headerContentType, fileUploadRequest, cancellationToken);
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

        private ObjectResult? ValidateContentDisposition(MultipartSection section, string? headerContentType)
        {
            var contentDisposition = section.GetContentDispositionHeader();

            var fileName = MultipartRequestHelper.GetFileContentDisposition(contentDisposition);
            if (!string.IsNullOrEmpty(fileName))
            {
                var contentTypeProvider = new FileExtensionContentTypeProvider();
                contentTypeProvider.TryGetContentType(fileName, out var fileContentType);

                logger.LogDebug("Header content type :{headerContentType}, File content type: {fileContentType}", headerContentType, fileContentType);
                if (headerContentType == fileContentType)
                {
                    if (headerContentType == null || !options.Value.MimeTypes.Contains(headerContentType))
                    {
                        logger.LogWarning("File upload attempt will not continue due to unauthorized MimeType: {headerContentType}", headerContentType);
                        return Problem($"Unauthorized file extension. Extension: {Path.GetExtension(fileName)}");
                    }
                }
                else
                {
                    logger.LogWarning("Inconsitency validating file content type.");
                    return Problem("Unable to validate content disposition header.");
                }
            }

            return null;
        }

        private async Task<CreateDocumentResponse?> HandleFileUpload(Stream? fileStream, string? contentType, FileUploadRequest fileUploadRequest, CancellationToken cancellationToken)
        {
            if (fileStream != null && contentType != null)
            {
                logger.LogDebug("Starting file upload service");
                return await documentService.UploadFileAsync(fileStream, contentType, fileUploadRequest, cancellationToken);
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
