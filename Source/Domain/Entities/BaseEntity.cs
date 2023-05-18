namespace Domain.Entities
{
    public record BaseEntity<T>
    {
        public T Id { get; set; }
    }
}
