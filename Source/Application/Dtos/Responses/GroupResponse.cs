using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record GroupResponse : AuditableEntityDto<int>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GroupResponse() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public GroupResponse(Group group)
        {
            Id = group.Id;
            Name = group.Name;
            InsertedAt = group.Inserted_At;
            InsertedBy = group.Inserted_By;
            UpdatedAt = group.Updated_At;
            UpdatedBy = group.Updated_By;
            Users =  group.Users?.Select(u => u.User_Id);
        }

        public string Name { get; set; }

        public IEnumerable<int>? Users { get; set; }
    }
}
