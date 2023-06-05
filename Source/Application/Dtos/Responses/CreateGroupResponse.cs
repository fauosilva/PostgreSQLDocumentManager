using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record CreateGroupResponse : AuditableEntityDto<int>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CreateGroupResponse() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public CreateGroupResponse(Group group)
        {
            Id = group.Id;
            Name = group.Name;
            InsertedAt = group.Inserted_At;
            InsertedBy = group.Inserted_By;
        }

        public string Name { get; set; }
    }
}
