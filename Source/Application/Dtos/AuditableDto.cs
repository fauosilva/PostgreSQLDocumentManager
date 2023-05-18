namespace Application.Dtos
{
    public record AuditableDto<T> : BaseDto<T>
    {
        public DateTimeOffset InsertedAt { get; set; }
        public string InsertedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
