namespace ApplicationCore.Constants
{
    //Not using default claims (ex:DefaultNameClaimType) to lower jwt size.
    public static class UserClaimsConstants
    {
        public const string UserId = "id";
        public const string UserName = "name";
        public const string Role = "role";
    }
}
