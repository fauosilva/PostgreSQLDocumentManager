namespace ApplicationCore.Entities
{
    /// <summary>
    /// User records that are stored on the database
    /// </summary>
    public record User : AuditableEntity<int>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public override string ToString()
        {
            return $"{{{nameof(Id)}={Id}, {nameof(Username)}={Username}, {nameof(Password)}={Password}, {nameof(Role)}={Role}, {nameof(Inserted_At)}={Inserted_At}, {nameof(Inserted_By)}={Inserted_By}, {nameof(Updated_At)}={Updated_At}, {nameof(Updated_By)}={Updated_By}}}";
        }
    }
}
