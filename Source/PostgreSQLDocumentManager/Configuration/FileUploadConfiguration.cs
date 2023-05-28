using System.ComponentModel.DataAnnotations;

namespace PostgreSQLDocumentManager.Configuration
{
    public class FileUploadConfiguration
    {
        public const string ConfigSection = "FileUpload";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Required]
        public HashSet<string> MimeTypes { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
