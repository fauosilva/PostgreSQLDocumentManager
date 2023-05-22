namespace ApplicationCore.Entities
{
    public record BaseEntity<TKeyType> where TKeyType : notnull
    {
        public TKeyType Id { get; set; }
    }
}
