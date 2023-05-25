using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Dtos;

namespace ApplicationCore.Dtos.Responses
{
    public record FileUploadResponse : AuditableEntityDto<int>
    {
        public FileUploadResponse(Document document)
        {
            Id = document.Id;
            Name = document.Name;
            Category = document.Category;
            Description = document.Description;
            InsertedAt = document.Inserted_At;
            InsertedBy = document.Inserted_By;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
