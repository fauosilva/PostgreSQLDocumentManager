namespace ApplicationCore.Interfaces.Dtos
{
    public interface IAuditableEntityDto
    {
        DateTime InsertedAt { get; set; }
        string InsertedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string? UpdatedBy { get; set; }
    }
}