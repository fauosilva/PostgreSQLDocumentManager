using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Dtos.Requests
{
    public record PermissionRequest : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int? UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int? GroupId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!UserId.HasValue && !GroupId.HasValue)
            {
                yield return new ValidationResult("UserId or GroupId must be provided.", new string[] { nameof(UserId), nameof(GroupId) });
            }

            if (UserId.HasValue && GroupId.HasValue)
            {
                yield return new ValidationResult("UserId and GroupId were provided on the same request. Please send only one on the request.", new string[] { nameof(UserId), nameof(GroupId) });
            }
        }
    }
}
