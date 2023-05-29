using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record CreateUserGroupResponse : CompositeKeyAuditableEntityDto
    {
        public CreateUserGroupResponse(UserGroup userGroup)
        {
            UserId = userGroup.User_Id;
            GroupId = userGroup.Group_Id;
            InsertedAt = userGroup.Inserted_At;
            InsertedBy = userGroup.Inserted_By;
        }        
        public int? UserId { get; set; }
        public int? GroupId { get; set; }
    }
}
