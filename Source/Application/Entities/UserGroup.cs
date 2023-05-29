namespace ApplicationCore.Entities
{
    public record UserGroup : CompositeKeyAuditableEntity
    {
        public int User_Id { get; set; }
        public int Group_Id { get; set; }
    }
}
