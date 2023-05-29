using System;
using TechTalk.SpecFlow;

namespace EndToEndTests.StepDefinitions
{
    [Binding]
    public class FileUploadStepDefinitions
    {
        private readonly ScenarioContext scenarioContext;

        public FileUploadStepDefinitions(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [Given(@"an user with role was created")]
        public void GivenAnUserWithRoleWasCreated(Table table)
        {
            throw new PendingStepException();
        }

        [Given(@"a JWT token was generated for the created user with their credentials")]
        public void GivenAJWTTokenWasGeneratedForTheCreatedUserWithTheirCredentials(Table table)
        {
            throw new PendingStepException();
        }

        [Given(@"a file was uploaded by the Admin user")]
        public void GivenAFileWasUploadedByTheAdminUser(Table table)
        {
            throw new PendingStepException();
        }

        [Given(@"the created user was given direct access to the file")]
        public void GivenTheCreatedUserWasGivenDirectAccessToTheFile(Table table)
        {
            throw new PendingStepException();
        }

        [When(@"the created user attempts to download the file")]
        public void WhenTheCreatedUserAttemptsToDownloadTheFile()
        {
            throw new PendingStepException();
        }

        [Then(@"the created user should exist on the database")]
        public void ThenTheCreatedUserShouldExistOnTheDatabase()
        {
            throw new PendingStepException();
        }

        [Then(@"the created user should have access granted to the file on the database")]
        public void ThenTheCreatedUserShouldHaveAccessGrantedToTheFileOnTheDatabase()
        {
            throw new PendingStepException();
        }

        [Then(@"the created user should be able to download the file")]
        public void ThenTheCreatedUserShouldBeAbleToDownloadTheFile(Table table)
        {
            throw new PendingStepException();
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
