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
6. Infrastructure



## Database decisions

1. C# string -> PostgreSQL text data type
	1. This choice of database collumn type was made since restricting the size of the collumns on the database is a little bit less performatic.
	[Link](https://www.postgresql.org/docs/current/datatype-character.html)

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

