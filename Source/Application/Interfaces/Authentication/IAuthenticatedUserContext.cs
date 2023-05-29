namespace ApplicationCore.Interfaces.Authentication
{
    public interface IAuthenticatedUserContext
    {
        int GetUserId();
        string GetUserName();        
        string GetUserRole();
    }
}
