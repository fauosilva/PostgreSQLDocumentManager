using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Dtos.Requests
{
    public class FileUploadRequest : IValidatableObject
    {
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? Category { get; private set; }

        private string? keyName;
        
        public void AddMetadata(string key, string value)
        {
            if (string.Equals(key, nameof(Name), StringComparison.InvariantCultureIgnoreCase))
            {
                Name = value;
            }

            if (string.Equals(key, nameof(Description), StringComparison.InvariantCultureIgnoreCase))
            {
                Description = value;
            }

            if (string.Equals(key, nameof(Category), StringComparison.InvariantCultureIgnoreCase))
            {
                Category = value;
            }
        }

        public string GetKeyName()
        {
            return keyName ??= DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + Name;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Name))
            {
                yield return new ValidationResult("File name must be provided.", new string[] { nameof(Name) });
            }

            if (string.IsNullOrEmpty(Description))
            {
                yield return new ValidationResult("File description must be provided.", new string[] { nameof(Description) });
            }

            if (string.IsNullOrEmpty(Category))
            {
                yield return new ValidationResult("File category must be provided.", new string[] { nameof(Category) });
            }
        }
    }
}
