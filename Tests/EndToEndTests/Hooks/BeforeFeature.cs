using EndToEndTests.Configuration;
using Npgsql;
using System.Text.Json;

namespace EndToEndTests
{
    [Binding]
    public static class BeforeFeature
    {
        public const string FeatureContextHttpClient = nameof(FeatureContextHttpClient);
        public const string DataSource = nameof(DataSource);
        public const string TestSettings = nameof(TestSettings);

        [BeforeFeature]
        public static async Task BeforeFeatureInitializer(FeatureContext featureContext)
        {
            var configuration = await ReadAsync<TestSettings>("Testsettings.json");
            if (configuration != null)
            {
                featureContext.Add(TestSettings, configuration);

                var handler = new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10) // Recreate every 15 minutes
                };
                var sharedClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(configuration.Api.Url)
                };

                featureContext.Add(FeatureContextHttpClient, sharedClient);

                var npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.ConnectionStrings.DocumentManager);
                var npgSqlDataSource = npgsqlDataSourceBuilder.Build();
                featureContext.Add(DataSource, npgSqlDataSource);
            }
        }

        [AfterFeature]
        public static void AfterFeatureInitializer(FeatureContext featureContext)
        {
            var httpClient = featureContext.Get<HttpClient>(FeatureContextHttpClient);
            httpClient.Dispose();

            var dataSource = featureContext.Get<NpgsqlDataSource>(DataSource);
            dataSource.Dispose();

        }

        public static async Task<T?> ReadAsync<T>(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
    }
}
