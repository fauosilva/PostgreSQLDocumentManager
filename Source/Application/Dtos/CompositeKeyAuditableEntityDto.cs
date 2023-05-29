using ApplicationCore.Interfaces.Dtos;

namespace ApplicationCore.Dtos
{
    public record CompositeKeyAuditableEntityDto : IAuditableEntityDto
    {
        public DateTime InsertedAt { get; set; }
        public string InsertedBy { get; set; } = default!;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

}
