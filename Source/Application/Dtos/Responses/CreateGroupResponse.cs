using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record CreateGroupResponse : AuditableEntityDto<int>
    {
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
