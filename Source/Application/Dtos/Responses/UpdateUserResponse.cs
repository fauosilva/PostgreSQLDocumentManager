using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record UpdateUserResponse : AuditableEntityDto<int>
    {
        public UpdateUserResponse(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Role = user.Role;
            InsertedAt = user.Inserted_At;
            InsertedBy = user.Inserted_By;
            UpdatedAt = user.Updated_At;
            UpdatedBy = user.Updated_By;
        }

        public string Username { get; set; }
        public string Role { get; set; }
    }
}
