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
        private readonly RestApiHttpClient restApiHttpClient;        

        public AfterScenario(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            this.scenarioContext = scenarioContext;        
            var testSettings = featureContext.Get<TestSettings>(BeforeFeature.TestSettings);
            var httpClient = featureContext.Get<HttpClient>(BeforeFeature.FeatureContextHttpClient);
            restApiHttpClient = new RestApiHttpClient(httpClient, testSettings);            
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
                //So far the API does not support document delete ;)
            }

            scenarioContext.TryGetValue<CreateGroupResponse>(ScenarioContextConstants.CreateGroupResponse, out var createGroupResponse);
            if (createGroupResponse != null)
            {
                var result = await restApiHttpClient.DeleteGroupAsync(createGroupResponse.Id);
                result.Should().BeTrue($"Unable to delete group with Id: {createGroupResponse.Id}");
            }
        }

    }
}
