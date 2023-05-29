using ApplicationCore.Interfaces.Entities;

namespace ApplicationCore.Entities
{
    /// <summary>
    /// Entity that contains basic audit properties and has a composite key
    /// </summary>
    public record CompositeKeyAuditableEntity : IAuditableEntity 
    {
        public DateTime Inserted_At { get; set; }
        public string Inserted_By { get; set; } = string.Empty;
        public DateTime? Updated_At { get; set; }
        public string? Updated_By { get; set; }
    }
}
