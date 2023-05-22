namespace ApplicationCore.Entities
{
    /// <summary>
    /// Entity that contains basic audit properties.
    /// </summary>
    public record AuditableEntity<T> : BaseEntity<T> where T : notnull
    {
        public DateTime Inserted_At { get; set; }
        public string Inserted_By { get; set; } = string.Empty;
        public DateTime? Updated_At { get; set; }
        public string? Updated_By { get; set; }
    }
}
