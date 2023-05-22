namespace ApplicationCore.Dtos
{
    public record AuditableEntityDto<TKey> : BaseDto<TKey> where TKey : notnull
    {
        public DateTimeOffset InsertedAt { get; set; }
        public string InsertedBy { get; set; } = default!;
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
