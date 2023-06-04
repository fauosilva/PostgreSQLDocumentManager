using ApplicationCore.Dtos.Requests;

namespace EndToEndTests.Tables
{
    public class UserNameCreationTable
    {
        public string UserName { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
        public Role Role { get; set; } = Role.User;


        public CreateUserRequest ToRequest() => new() { Username = UserName, Password = UserPassword, Role = Role };
    }
}
