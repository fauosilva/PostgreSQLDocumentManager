# PostgreSQL Document Manager


## Architecture Decisions

1. Onion Architecture
	1. Due to the relative simplicity of the features implemented on the project a simple flavor of the Onion Architecture was used on this project
2. Domain 
	1. DDD was not implemented along the Onion Structure and an anemic model was built for this project. To invert this decision all the authorization logic and business rules should be migrated from the Application and Service layer to the domain, along with other adjusts to comply with DDD best-pratices.
	2. All entities that are persisted on the database can implement simple in-table audit fields.
3. Application / Controllers
    1. Used automapper to simplify mapping code for the service layer
	2. Input validation was implemented using data annotations for on the request Dtos.
	3. Input lenght was considered input validation and not a business logic for this project.
	4. Business logic validations was implemented on the service layer.
	5. Authorization logic was implemented on the controller layer.
4. Infrastructure
	1. In-table audit fields where implemented using stored procedures.

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

