using ApplicationCore.Entities;

namespace ApplicationCore.Dtos.Responses
{
    public record CreateDocumentResponse : AuditableEntityDto<int>
    {
        public CreateDocumentResponse() { }

        public CreateDocumentResponse(Document document)
        {
            Id = document.Id;
            Name = document.Name;
            KeyName = document.KeyName;
            Category = document.Category;
            Description = document.Description;
            Uploaded = document.Uploaded;
            InsertedAt = document.Inserted_At;
            InsertedBy = document.Inserted_By;
            UpdatedAt = document.Updated_At;
            UpdatedBy = document.Updated_By;
        }

        public string Name { get; set; }
        public string KeyName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool Uploaded { get; set; }
    }
}
