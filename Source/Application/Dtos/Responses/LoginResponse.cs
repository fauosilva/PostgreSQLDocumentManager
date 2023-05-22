namespace ApplicationCore.Dtos.Responses
{
    public class LoginResponse
    {
        public LoginResponse(string jwtToken)
        {
            JwtToken = jwtToken;
        }

        public string JwtToken { get; set; }
    }
}
