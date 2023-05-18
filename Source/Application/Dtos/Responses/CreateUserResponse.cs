namespace Application.Dtos.Responses
{
    public record CreateUserResponse : AuditableDto<int>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
