using Microsoft.Net.Http.Headers;

namespace PostgreSQLDocumentManager.Utilities
{
    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string? contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
        }

        public static string? GetFileContentDisposition(ContentDispositionHeaderValue? contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            if(contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data"))
            {
                if(!string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    return contentDisposition.FileName.Value;
                
                if (!string.IsNullOrEmpty(contentDisposition.FileNameStar.Value))
                    return contentDisposition.FileNameStar.Value;
            }

            return null;
        }
    }
}
