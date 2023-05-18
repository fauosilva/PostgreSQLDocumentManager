namespace Domain.Entities
{
    /// <summary>
    /// User records that are stored on the database
    /// </summary>
    public record User : AuditableEntity<int>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
