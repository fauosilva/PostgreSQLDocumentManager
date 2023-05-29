using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record CreatePermissionResponse : AuditableEntityDto<int>
    {
        public CreatePermissionResponse(DocumentPermission documentPermission)
        {
            Id = documentPermission.Id;
            DocumentId = documentPermission.Document_Id;
            UserId = documentPermission.User_Id;
            GroupId = documentPermission.Group_Id;
            InsertedAt = documentPermission.Inserted_At;
            InsertedBy = documentPermission.Inserted_By;
        }
        public int DocumentId { get; set; }
        public int? UserId { get; set; }
        public int? GroupId { get; set; }
    }
}
