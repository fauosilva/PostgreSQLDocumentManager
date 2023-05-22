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
5. Services
	1. Single database operations using the repository layer are not using the transaction block, or unit of work pattern since PostgreSQL MVCC actually handle those statements into a implicit transaction.
6. Repositories / Database
	1. Database unique constraints are used to mantain integrity of the entities. 

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
    - Open the project using your favorite terminal
	- Execute docker-compose to create a new container with the PostgreSQL database
	  - Wait for the compose to finish
	- Execute the project using dotnet run OR start the project using your favorite IDE.

## Development Environment

- Public URL


--- 

## Improvements

- Logs
    - [ ] Improve log scopes
	- [ ] Propagate log scopes from the controllers
- API Design
	- [ ] Return list of entities using pagination.
- Security
    - [ ] Rate limit on login requests based on origin.
