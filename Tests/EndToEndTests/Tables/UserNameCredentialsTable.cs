using ApplicationCore.Dtos.Requests;

namespace EndToEndTests.Tables
{
    public class UserNameCredentialsTable
    {
        public string UserName { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;

        public LoginRequest ToRequest() => new() { Username = UserName, Password = UserPassword};
    }
}
