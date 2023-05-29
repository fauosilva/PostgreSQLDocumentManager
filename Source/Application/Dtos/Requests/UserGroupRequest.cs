using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Dtos.Requests
{
    public class UserGroupRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int UserId { get; set; }
    }
}
