namespace EndToEndTests.Configuration
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class TestSettings
    {
        public Api Api { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }

    public class ConnectionStrings
    {
        public string DocumentManager { get; set; }
    }

    public class Api
    {
        public string Url { get; set; }
        public string AdminUser { get; set; }
        public string AdminPassword { get; set; }   
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
