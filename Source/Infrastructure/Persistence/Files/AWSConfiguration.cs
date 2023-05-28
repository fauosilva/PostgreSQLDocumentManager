using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Files
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable S101 // Types should be named in PascalCase
    public class AWSConfiguration
#pragma warning restore S101 // Types should be named in PascalCase
    {
        public const string ConfigSection = "Aws";

        [Required]
        public S3Configuration S3 { get; set; }
    }

    public class S3Configuration
    {
        [Required]
        public string Region { get; set; }
        [Required]
        public string BucketName { get; set; }
        [Required]
        public string AccessKeyId { get; set; }
        [Required]
        public string AccessKeySecret { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
