# PostgreSQL Document Manager


## Architecture Decisions

1. Onion Architecture
	1. Due to the relative simplicity of the features implemented on the project a simple flavor of the Onion Architecture was used.
2. Domain 
	1. DDD was not implemented along the Onion Structure and an anemic model was built for this project. To invert this decision all the authorization logic and business rules should be migrated from the Application and Service layer to the domain, along with other adjusts to comply with DDD best-pratices.
	2. All entities that are persisted on the database are implementing simple in-table audit fields.
3. Validation logic
	1. Input validation was implemented using data annotations on the request Dtos.
		1. Input lenght and other simple validations were considered input validation and not a business logic for this project.
	3. Business logic validations were implemented on the service layer.
	4. Authorization logic was implemented on the controller layer.
4. Controllers
    1. Exceptions propagated to the controller layer are handled by the exception handler returning a slighly enhanced problem details object.
5. Services
	1. Single database operations using the repository layer are not using the transaction block, or unit of work pattern since PostgreSQL MVCC actually handle those statements into a implicit transaction.
6. Repositories / Database
	1. Database unique constraints are used to mantain integrity of the entities.
	2. Unique indexes were not created to avoid index duplication. [Link](https://www.postgresql.org/docs/current/indexes-unique.html#:~:text=PostgreSQL%20automatically%20creates%20a%20unique,mechanism%20that%20enforces%20the%20constraint.)

## Database decisions

1. C# string -> PostgreSQL text data type
	1. This choice of database collumn type was made since restricting the size of the collumns on the database is a little bit less performatic.
	[Link](https://www.postgresql.org/docs/current/datatype-character.html)
2. Decided to use BCL Date types, despite NpgSQL recomending the utilization of NodaTime. [Link](https://www.npgsql.org/doc/types/nodatime.html?tabs=datasource)
3. Not using ON CONFLICT to describe behaviors different than throwing exceptions on INSERT/UPDATE conflicts.
    1. In case of concurrent operations violating unique database constraints, an 500 status code might be returned to the client due to the exception propagation. Those scenarios were not handled since they can be considered rare scenarios on this API.
		1. Example: Two users attempting to be registered with the same username on the same instant.

---

## Running the project

### Local
 
 - Requirements
	- docker-compose
 - Instructions
    - Open the project folder using your favorite terminal.
	- Execute docker-compose (docker-compose up) to create a new container with the PostgreSQL database
	  - Wait for the compose to finish and startup the PostgreSQL database.
	    - The seed data will include an Admin user with simple credentials (Username = adminuser / Password = Adminpassword)
	- Configure project secrets using CLI or using your favorite IDE

```json
{
  "Jwt": {
    "Key": "{secrets.json}",
    "Issuer": "{secrets.json}",
    "Audience": "{secrets.json}"
  },
  "AWS": {
    "S3": {
      "Region": "{secrets.json}",
      "BucketName": "{secrets.json}",
      "AccessKeyId": "{secrets.json}",
      "AccessKeySecret": "{secrets.json}"
    }
  }
}
```


- Execute the project using dotnet run OR start the project using your favorite IDE.


--- 

## Improvements

- Logs
    - [ ] Implement log scopes to enrich the logged information.
- API Design
	- [ ] Return list of entities using pagination.
	- [ ] Revoke document permissions.
- Security
    - [ ] Rate limit on login requests based on origin.
	- [ ] Implement refresh token.
	- [ ] Enfoce HTTPS traffic.
- File Upload Security
	- [ ] Improve file extension validation by checking file signature. [link](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-7.0#file-signature-validation)
- Hosting
    - [ ] Implement health check

## Requirements
- Database
    - [x] Create stored procedures to implement some functionalities.
- Unit test
    - [x] Cover project with unit tests
- End to end tests
    - [x] Create end to end test scenario