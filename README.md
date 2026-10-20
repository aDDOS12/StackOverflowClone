# StackOverflowClone
[![CI](https://github.com/aDDOS12/StackOverflowClone/actions/workflows/ci.yml/badge.svg)](https://github.com/aDDOS12/StackOverflowClone/actions/workflows/ci.yml)
# This project aims to recreate the backend functionality of Stack Overflow.

## About

A REST API inspired by Stack Overflow, built with ASP.NET Core and Entity Framework Core. The goal is to practice production-grade backend development: layered architecture, data integrity, testing and CI.
This project started as a practice exercise in an Entity Framework Core course. It has since been redesigned (Clean Architecture, soft delete, auditing) and extended well beyond the original scope.

*This project is not affiliated with Stack Overflow.*

## Features

**Foundation**
-  Domain model and database schema with EF Core migrations
-  Soft delete and automatic timestamps
-  API documentation (OpenAPI + Scalar)

**Users**
-  User registration (`POST /api/users`)
-  Login with JWT (`POST /api/auth/login`)
-  Current user profile (`GET /api/users/me`)
-  Account deletion with personal data anonymization (GDPR) (`POST /api/users/me/delete`)

**Content**
-  Questions: create, get, edit and delete (authors only)
-  Browsing tags (`GET /api/tags`)
-  Question list with pagination, sorting (newest, score) and tag filtering
-  Answers: create, paginated list, edit and delete (authors only)
-  Comments on questions and answers

**Business logic**
-  Upvotes and downvotes
-  Accepting answers
-  Deletion rules for questions with valuable answers

**Testing and DevOps**
-  Unit tests and integration tests against a real SQL Server (Testcontainers)
-  Continuous integration with GitHub Actions
-  Docker Compose setup: SQL Server, a one-off migration container and the API

## Tech stack

- .NET 10, C# 14
- ASP.NET Core Web API (controllers)
- Entity Framework Core 10
- SQL Server (LocalDB for development)
- OpenAPI (`Microsoft.AspNetCore.OpenApi`) + Scalar
- FluentValidation
- JWT bearer authentication 
- xUnit v3, NSubstitute, Testcontainers, Respawn
- Docker, Docker Compose, GitHub Actions

## Architecture

The solution follows Clean Architecture. Dependencies point inwards:

```
Api ──► Application ──► Domain
 │                        ▲
 └──► Infrastructure ─────┘
```

- **Domain** – entities and core rules. No external dependencies.
- **Application** – use cases, DTOs and validation.
- **Infrastructure** – EF Core: `DbContext`, migrations, interceptors, seed data.
- **Api** – controllers, configuration and dependency injection setup.

## Key decisions

- **Soft delete** – questions, answers, comments and users are never physically deleted. A `DeletedAt` timestamp is set instead, and named global query filters hide deleted records.
- **Restricted deletes** – foreign keys use `Restrict` instead of cascade, so the database blocks accidental hard deletes.
- **Auditing via interceptor** – a `SaveChangesInterceptor` sets `CreatedAt`, `UpdatedAt` and `DeletedAt` in one place, using `TimeProvider` for testability.
- **Comment integrity** – a check constraint guarantees that a comment belongs to exactly one parent: a question or an answer.
- **No lazy loading** – related data is loaded explicitly to avoid N+1 queries.
- **Resource-level authorization** – `[Authorize]` only checks that a user is logged in; services check that the user is the author of the specific resource and return `403 Forbidden` otherwise.
- **Atomic score updates** – votes change scores with a single SQL `UPDATE ... SET Score = Score + @delta` inside a transaction, which avoids lost updates under concurrent voting.
- **Machine-readable error codes** – conflicts return a standard `409` with a `code` field in ProblemDetails (e.g. `email_taken`), instead of custom HTTP status codes.
- **Real database in tests** – integration tests run against SQL Server in a container instead of an in-memory provider, so constraints, filtered indexes and transactions behave as in production.
- **Migrations as a separate step** – the API never changes the database schema. A dedicated container applies migrations with an EF Core migration bundle, and the API starts only after it succeeds.

Detailed reasoning is recorded as Architecture Decision Records in [`docs/decisions`](docs/decisions).

## Getting started

There are two ways to run the project:

- **Docker Compose** – the quickest way to try the API. Only Docker is required.
- **Local development** – for working on the code in Visual Studio or with the .NET CLI.

### Option A: Docker Compose (recommended)

**Prerequisites:** [Docker](https://www.docker.com/)

1. Clone the repository:
```
   git clone https://github.com/aDDOS12/StackOverflowClone.git
   cd StackOverflowClone
```
2. Create a `.env` file from the example:
```
   cp .env.example .env
```
   (PowerShell: `Copy-Item .env.example .env`)

3. Set the values in `.env`:
   - `DB_PASSWORD` – must meet the SQL Server password policy: at least 8 characters, including three of: uppercase letters, lowercase letters, digits and symbols. With a weak password the database container stops right after starting.
   - `JWT_SIGNING_KEY` – a random string of at least 32 characters (see the key generation command below).

4. Start the system:
```
   docker compose up --build -d
```
   Compose starts SQL Server, waits until it is healthy, applies the migrations in a one-off `migrator` container, and then starts the API.

5. Open the API reference at `http://localhost:8080/scalar/v1`.

Useful commands:
```
docker compose ps -a          # status of all containers (the migrator should show "Exited (0)")
docker compose logs -f api    # follow API logs
docker compose down           # stop and remove containers (data is kept in a volume)
docker compose down -v        # also delete the database volume
```

> Port `1433` is published so that you can connect to the database from your machine. If another SQL Server is already using it, stop that instance or change the port mapping in `docker-compose.yml`.

### Option B: Local development

**Prerequisites:**

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (Windows; installed with Visual Studio or SQL Server Express)
- [Docker](https://www.docker.com/) (for integration tests)

1. Clone the repository:
```
   git clone https://github.com/aDDOS12/StackOverflowClone.git
   cd StackOverflowClone
```
2. Configure the JWT signing key (stored outside the repository with User Secrets).

Generate a random key (PowerShell):
```
   $bytes = New-Object byte[] 64; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes); [Convert]::ToBase64String($bytes)
```
   Then save it:
```
   dotnet user-secrets set "Jwt:SigningKey" "<generated-key>" --project src/StackOverflowClone.Api
```
3. Create the database by applying migrations, using one of the options below.

   **Visual Studio (Package Manager Console)** – set *Default project* to `StackOverflowClone.Infrastructure`, then run:
```
   Update-Database
```
   **.NET CLI:**
```
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project src/StackOverflowClone.Infrastructure --startup-project src/StackOverflowClone.Api
```
4. Run the API:
```
   dotnet run --project src/StackOverflowClone.Api --launch-profile https
```
5. Open the API reference at `https://localhost:7266/scalar/v1`.

The connection string is configured in `src/StackOverflowClone.Api/appsettings.Development.json`.

If the browser warns about the certificate, trust the .NET development certificate once:
```
   dotnet dev-certs https --trust
```

### Run tests

```
dotnet test
```

Integration tests run against a real SQL Server started automatically in a Docker container ([Testcontainers](https://dotnet.testcontainers.org/)), so Docker must be running.

## Roadmap

- [x] Phase 0 – Data model fixes, Clean Architecture structure, README
- [x] Phase 1 – API foundation: first endpoints, error handling, validation
- [x] Phase 2 – Users and security: registration, JWT, authorization, account deletion (GDPR)
- [x] Phase 3 – Questions, answers, comments, tags, pagination 
- [x] Phase 4 – Business logic: voting, accepting answers
- [x] Phase 5 – Unit and integration tests
- [x] Phase 6 – Docker and GitHub Actions CI

## License

This project is licensed under the MIT License – see [LICENSE.txt](LICENSE.txt).
