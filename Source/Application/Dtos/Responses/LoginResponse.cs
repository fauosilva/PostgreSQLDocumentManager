namespace ApplicationCore.Dtos.Responses
{
    public record LoginResponse
    {
        public LoginResponse(string jwtToken)
        {
            JwtToken = jwtToken;
        }

        public string JwtToken { get; set; }
    }
}
