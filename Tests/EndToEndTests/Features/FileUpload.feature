Feature: FileUpload

@Cleanup
Scenario: User should be able to download file after being set download permissions
	And an user with role was created
		| userName | userPassword     | role |
		| testUser | testUserPassword | User |
	And a JWT token was generated for the created user with their credentials
		| userName | userPassword     |
		| testUser | testUserPassword |
	And a file was uploaded by the Admin user
		| fileName   | name   | description           | category       | mimeType  |
		| sample.png | sample | testSampleDescription | sampleCategory | image/png |
	And the created user was given direct access to the uploaded file
	When the created user attempts to download the file
	Then the created user should be able to download the file
		| fileName   | mimeType  |
		| sample.png | image/png |
	And the created user should exist on the database
	And the created user should have access granted to the file on the database
	
	

Scenario: User should be able to download file after being set download permissions through a group
	And an user with role was created
		| userName | userPassword     | role |
		| testUser | testUserPassword | User |
	And and a group with name '<sample>' was created
	And the created user was assigned to the created group
		| groupName | userName  |
		| sample    | testAdmin |
	And a JWT token was generated for the created user with their credentials
		| userName | userPassword     |
		| testUser | testUserPassword |
	And a file was uploaded by the Admin user
		| fileName    | name    | description           | category       | mimeType  |
		| sample2.png | sample2 | testSampleDescription | sampleCategory | image/png |
	And the created group was given access to the file
		| fileName   | groupName |
		| sample.png | sample    |
	When the created user attempts to download the file
	Then the created user should be able to download the file
		| fileName   | mimeType  |
		| sample.png | image/png |
	And the created user should exist on the database
	And the created group should exist on the database
	And the user should be associated with the group on the database
	And the created group should have access granted to the file on the database
	
	