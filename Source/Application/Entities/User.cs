namespace ApplicationCore.Entities
{
    /// <summary>
    /// User records that are stored on the database
    /// </summary>
    public record User : AuditableEntity<int>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
