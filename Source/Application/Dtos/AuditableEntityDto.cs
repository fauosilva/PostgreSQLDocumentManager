namespace ApplicationCore.Dtos
{
    public record AuditableEntityDto<TKey> : BaseDto<TKey> where TKey : notnull
    {
        public DateTime InsertedAt { get; set; }
        public string InsertedBy { get; set; } = default!;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
