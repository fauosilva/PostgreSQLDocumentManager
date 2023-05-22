namespace ApplicationCore.Dtos
{
    public record BaseDto<TKey> where TKey : notnull
    {
        public TKey Id { get; set; } = default!;        
    }
}
