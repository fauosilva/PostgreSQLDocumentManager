namespace ApplicationCore.Interfaces.Authentication
{
    public interface IAuthenticatedUserContext
    {
        string GetUserName();
        string GetUserId();
        string GetUserRole();
    }
}
