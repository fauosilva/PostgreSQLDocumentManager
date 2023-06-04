namespace EndToEndTests.Configuration
{
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
}
