namespace ApplicationCore.Entities
{
    /// <summary>
    /// Entity that contains basic audit properties.
    /// </summary>
    public record AuditableEntity<T> : BaseEntity<T>
    {
        public DateTime Inserted_At { get; set; }
        public string Inserted_By { get; set; }
        public DateTime? Updated_At { get; set; }
        public string? Updated_By { get; set; }
    }
}
