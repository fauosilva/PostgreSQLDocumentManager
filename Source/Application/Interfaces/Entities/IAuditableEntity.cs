namespace ApplicationCore.Interfaces.Entities
{
    public interface IAuditableEntity
    {
        DateTime Inserted_At { get; set; }
        string Inserted_By { get; set; }
        DateTime? Updated_At { get; set; }
        string? Updated_By { get; set; }
    }
}
