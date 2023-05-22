using System.ComponentModel.DataAnnotations;

namespace PostgreSQLDocumentManager.Configuration
{
    public class JwtConfiguration
    {
        [Required]
        public string Key { get; set; } = string.Empty;
        [Required]
        public string Issuer { get; set; } = string.Empty;
        [Required]
        public string Audience { get; set; } = string.Empty;
    }
}
