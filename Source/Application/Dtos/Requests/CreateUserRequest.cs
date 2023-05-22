using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Dtos.Requests
{
    public class CreateUserRequest
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Required]
        [MaxLength(255)]
        [MinLength(4)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        [MinLength(8)]
        public string Password { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Required]
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }
    }

    public enum Role
    {
        User = 1,
        Manager = 2,
        Admin = 3
    }
}
