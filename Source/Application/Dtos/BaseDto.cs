namespace Application.Dtos
{
    public record BaseDto<T>
    {
        public T Id { get; set; }
    }
}
