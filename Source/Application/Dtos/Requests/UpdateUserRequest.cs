using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Dtos.Requests
{
    public class UpdateUserRequest
    {        
        [MaxLength(255)]
        [MinLength(8)]
        public string? Password { get; set; }
                
        public Role? Role { get; set; }
    }
}
