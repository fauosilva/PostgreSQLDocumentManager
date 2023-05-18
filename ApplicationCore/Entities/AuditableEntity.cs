namespace Domain.Entities
{
    /// <summary>
    /// Entity that contains basic audit properties.
    /// </summary>
    public record AuditableEntity<T> : BaseEntity<T>
    {
        public DateTimeOffset InsertedAt { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
