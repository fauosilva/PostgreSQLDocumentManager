using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record GroupResponse : AuditableEntityDto<int>
    {
        public GroupResponse(Group group)
        {
            Id = group.Id;
            Name = group.Name;
            InsertedAt = group.Inserted_At;
            InsertedBy = group.Inserted_By;
            UpdatedAt = group.Updated_At;
            UpdatedBy = group.Updated_By;
        }

        public string Name { get; set; }
    }
}
