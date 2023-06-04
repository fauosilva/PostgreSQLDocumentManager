using ApplicationCore.Dtos.Responses;
using EndToEndTests.Configuration;
using EndToEndTests.Constants;
using EndToEndTests.Tables;
using Npgsql;
using TechTalk.SpecFlow.Assist;

namespace EndToEndTests.StepDefinitions
{
    [Binding]
    public class FileUploadStepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly FeatureContext featureContext;
        private readonly RestApiHttpClient restApiHttpClient;
        private readonly NpgsqlDataSource npgsqlDataSource;


        public FileUploadStepDefinitions(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            this.scenarioContext = scenarioContext;
            this.featureContext = featureContext;
            var testSettings = featureContext.Get<TestSettings>(BeforeFeature.TestSettings);
            var httpClient = featureContext.Get<HttpClient>(BeforeFeature.FeatureContextHttpClient);
            restApiHttpClient = new RestApiHttpClient(httpClient, testSettings);
            npgsqlDataSource = featureContext.Get<NpgsqlDataSource>(BeforeFeature.DataSource);
        }

        [Given(@"an user with role was created")]
        public async Task GivenAnUserWithRoleWasCreated(Table table)
        {
            var createUserRequest = table.CreateInstance<UserNameCreationTable>().ToRequest();
            var response = await restApiHttpClient.CreateUserAsync(createUserRequest);
            response.Should().NotBeNull();
            response.Id.Should().BePositive();
            response.Username.Should().Be(createUserRequest.Username);
            response.Role.Should().Be(createUserRequest.Role.ToString());
            response.InsertedAt.Should().BeAfter(DateTime.UtcNow.AddSeconds(-10));
            response.InsertedBy.Should().NotBeNullOrEmpty();

            scenarioContext.Add(ScenarioContextConstants.CreateUserResponse, response);
        }

        [Given(@"a JWT token was generated for the created user with their credentials")]
        public async Task GivenAJWTTokenWasGeneratedForTheCreatedUserWithTheirCredentials(Table table)
        {
            var loginRequest = table.CreateInstance<UserNameCredentialsTable>().ToRequest();
            var jwtToken = await restApiHttpClient.GenerateJwtAsync(loginRequest);
            scenarioContext.Add("userJwt", jwtToken.JwtToken);
        }

        [Given(@"a file was uploaded by the Admin user")]
        public async Task GivenAFileWasUploadedByTheAdminUser(Table table)
        {
            var fileUploadRequest = table.CreateInstance<FileUploadRequestTable>().ToRequest();
            var fileUploadResponse = await restApiHttpClient.FileUploadAsync(fileUploadRequest);
            scenarioContext.Add(ScenarioContextConstants.CreateDocumentResponse, fileUploadResponse);
        }

        [Given(@"the created user was given direct access to the uploaded file")]
        public async Task GivenTheCreatedUserWasGivenDirectAccessToTheUploadedFile()
        {
            var createdDocument = scenarioContext.Get<CreateDocumentResponse>(ScenarioContextConstants.CreateDocumentResponse);
            var createdUser = scenarioContext.Get<CreateUserResponse>(ScenarioContextConstants.CreateUserResponse);
            await restApiHttpClient.AddUserPermissionAsync(createdDocument.Id, createdUser.Id);
        }

        [When(@"the created user attempts to download the file")]
        public async Task WhenTheCreatedUserAttemptsToDownloadTheFile()
        {
            var userJwt = scenarioContext.Get<string>("userJwt");
            var createdDocument = scenarioContext.Get<CreateDocumentResponse>(ScenarioContextConstants.CreateDocumentResponse);
            var downloadStream = await restApiHttpClient.DownloadDocumentAsync(createdDocument.Id, userJwt);
            scenarioContext.Add("downloadDocument", downloadStream);
        }

        [Then(@"the created user should exist on the database")]
        public void ThenTheCreatedUserShouldExistOnTheDatabase()
        {
            var createdUser = scenarioContext.Get<CreateUserResponse>(ScenarioContextConstants.CreateUserResponse);

            using var connection = npgsqlDataSource.OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT 1 FROM users WHERE id = @p1", connection);
            cmd.Parameters.AddWithValue("p1",  createdUser.Id);           
            int? returnValue = (int?)cmd.ExecuteScalar();
            returnValue.Should().Be(1);
        }

        [Then(@"the created user should have access granted to the file on the database")]
        public void ThenTheCreatedUserShouldHaveAccessGrantedToTheFileOnTheDatabase()
        {
            var createdUser = scenarioContext.Get<CreateUserResponse>(ScenarioContextConstants.CreateUserResponse);
            var createdDocument = scenarioContext.Get<CreateDocumentResponse>(ScenarioContextConstants.CreateDocumentResponse);

            using var connection = npgsqlDataSource.OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT 1 FROM document_permissions WHERE user_id = @p1 AND document_id = @p2", connection)
            {
                Parameters =
                {
                    new("p1", createdUser.Id),
                    new("p2", createdDocument.Id)
                }
            };
            int? returnValue = (int?)cmd.ExecuteScalar();
            returnValue.Should().Be(1);
        }

        [Then(@"the created user should be able to download the file")]
        public void ThenTheCreatedUserShouldBeAbleToDownloadTheFile(Table table)
        {
            var downloadStream = scenarioContext.Get<Stream>("downloadDocument");
            downloadStream.Should().NotBeNull();
            downloadStream.Dispose();
        }

        [Given(@"and a group with name '([^']*)' was created")]
        public void GivenAndAGroupWithNameWasCreated(string sample)
        {
            throw new PendingStepException();
        }

        [Given(@"the created user was assigned to the created group")]
        public void GivenTheCreatedUserWasAssignedToTheCreatedGroup(Table table)
        {
            throw new PendingStepException();
        }

        [Given(@"the created group was given access to the file")]
        public void GivenTheCreatedGroupWasGivenAccessToTheFile(Table table)
        {
            throw new PendingStepException();
        }

        [Then(@"the created group should exist on the database")]
        public void ThenTheCreatedGroupShouldExistOnTheDatabase()
        {
            throw new PendingStepException();
        }

        [Then(@"the user should be associated with the group on the database")]
        public void ThenTheUserShouldBeAssociatedWithTheGroupOnTheDatabase()
        {
            throw new PendingStepException();
        }

        [Then(@"the created group should have access granted to the file on the database")]
        public void ThenTheCreatedGroupShouldHaveAccessGrantedToTheFileOnTheDatabase()
        {
            throw new PendingStepException();
        }

    }
}
