using ApplicationCore.Dtos.Responses;
using EndToEndTests.Configuration;
using EndToEndTests.Constants;
using Npgsql;

namespace EndToEndTests.Hooks
{
    [Binding]
    public class AfterScenario
    {
        private readonly ScenarioContext scenarioContext;
        private readonly FeatureContext featureContext;
        private RestApiHttpClient restApiHttpClient;
        private NpgsqlDataSource npgsqlDataSource;

        public AfterScenario(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            this.scenarioContext = scenarioContext;
            this.featureContext = featureContext;            
            var testSettings = featureContext.Get<TestSettings>(BeforeFeature.TestSettings);
            var httpClient = featureContext.Get<HttpClient>(BeforeFeature.FeatureContextHttpClient);
            restApiHttpClient = new RestApiHttpClient(httpClient, testSettings);
            npgsqlDataSource = featureContext.Get<NpgsqlDataSource>(BeforeFeature.DataSource);
        }

        [AfterScenario("Cleanup")]
        public async Task RemoveCreatedEntities()
        {
            scenarioContext.TryGetValue<CreateUserResponse>(ScenarioContextConstants.CreateUserResponse, out var createUserResponse);
            if (createUserResponse != null)
            {
                var result = await restApiHttpClient.DeleteUserAsync(createUserResponse.Id);
                result.Should().BeTrue($"Unable to delete user with Userid: {createUserResponse.Id}");
            }

            scenarioContext.TryGetValue<CreateDocumentResponse>(ScenarioContextConstants.CreateDocumentResponse, out var createDocumentResponse);
            if (createDocumentResponse != null)
            {
                //var result = await restApiHttpClient.DeleteDocumentAsync(createDocumentResponse.Id);
                //result.Should().BeTrue($"Unable to delete document with Id: {createDocumentResponse.Id}");
            }            
        }

    }
}
