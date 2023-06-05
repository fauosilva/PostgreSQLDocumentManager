using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record CreateUserResponse : AuditableEntityDto<int>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CreateUserResponse() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public CreateUserResponse(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Role = user.Role;
            InsertedAt = user.Inserted_At;
            InsertedBy = user.Inserted_By;
        }

        public string Username { get; set; }
        public string Role { get; set; }
    }
}
