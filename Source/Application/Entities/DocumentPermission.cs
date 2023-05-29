namespace ApplicationCore.Entities
{
    public record DocumentPermission : AuditableEntity<int>
    {
        public int Document_Id { get; set; }
        public int? User_Id { get; set; }
        public int? Group_Id { get; set; }
    }
}
