# Product CRUD API

A RESTful backend API for managing Products, built with .NET 8 and a Clean Architecture layout (Domain / Application / Infrastructure / API).

## Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core 8 + SQL Server
- AutoMapper, FluentValidation
- JWT Bearer authentication (scaffolded)
- Swagger / OpenAPI (Swashbuckle)
- xUnit + Moq + WebApplicationFactory for tests
- Docker + Docker Compose

## Project Structure

```
ProductApi/
├── src/
│   ├── API/              # Controllers, middleware, DI wiring, Program.cs
│   ├── Application/      # DTOs, service interfaces/implementations, validators, mapping
│   ├── Domain/            # Entities, custom exceptions
│   └── Infrastructure/    # EF Core DbContext, entity configs, repositories
├── tests/
│   ├── Application.Tests/ # Unit tests (Moq)
│   └── API.Tests/         # Integration tests (WebApplicationFactory + in-memory DB)
├── docker-compose.yml
└── ProductApi.sln
```

## Running Locally (without Docker)

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download), a SQL Server instance (LocalDB, a container, or a full install), and the EF Core CLI tool.

1. Install the EF tool once, if you don't have it:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```

3. Update the connection string in `src/API/appsettings.json` (or `appsettings.Development.json`) to point at your SQL Server instance.

4. Create the initial migration and apply it:
   ```bash
   cd src/API
   dotnet ef migrations add InitialCreate --project ../Infrastructure --startup-project .
   dotnet ef database update --project ../Infrastructure --startup-project .
   ```

5. Run the API:
   ```bash
   dotnet run --project src/API
   ```

6. Open Swagger UI at `https://localhost:5081/swagger` (or the port shown in the console) to try the endpoints.

## Running with Docker Compose (recommended)

This spins up SQL Server and the API together, and the API applies EF Core migrations automatically on startup.

```bash
docker-compose up --build
```

- API: `http://localhost:8080/swagger`
- SQL Server: `localhost:1433` (sa / Your_password123 — change this for anything beyond local dev)

> Note: you still need to generate the migration once locally (step 4 above) before the first `docker-compose up`, since migration files are part of the source and are what `Database.Migrate()` applies at container startup.

## Running Tests

```bash
dotnet test
```

- `Application.Tests` — unit tests for `ProductService` using mocked repositories.
- `API.Tests` — integration tests that spin up the full API pipeline against an in-memory database.

## API Endpoints

| Method | Route                     | Description                  |
|--------|---------------------------|-------------------------------|
| GET    | /api/v1/products          | List products (paginated)    |
| GET    | /api/v1/products/{id}     | Get a single product         |
| POST   | /api/v1/products          | Create a product             |
| PUT    | /api/v1/products/{id}     | Update a product              |
| DELETE | /api/v1/products/{id}     | Delete a product              |

All list responses are paginated via `?pageNumber=1&pageSize=20`.

## Architecture Notes

- **Clean Architecture / layered design**: Domain has no dependencies; Application depends only on Domain; Infrastructure implements Application's interfaces; API composes everything via DI.
- **Repository pattern**: `IProductRepository` abstracts EF Core so the service layer is testable without a real database.
- **Validation**: FluentValidation validators run automatically via `AddFluentValidationAutoValidation()`, returning 400 with field-level errors before the service layer is even hit.
- **Error handling**: a single `ExceptionHandlingMiddleware` converts domain exceptions (e.g. `NotFoundException`) into consistent JSON error responses with correct status codes.
- **Performance**: `AsNoTracking()` on read queries, pagination on the list endpoint, response compression, and async/await throughout.

## Security Notes

The JWT scaffolding is in place (`AddJwtAuthentication`) but no `[Authorize]` attributes are applied to endpoints by default, to keep the assessment easy to test via Swagger without needing a token flow. In a real deployment you would:

- Apply `[Authorize]` to mutating endpoints (`POST`/`PUT`/`DELETE`).
- Replace the placeholder `Jwt:Key` in `appsettings.json` with a secret from environment variables / a secrets manager.
- Restrict the CORS policy to known origins instead of `AllowAnyOrigin()`.
