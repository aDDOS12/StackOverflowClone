# StackOverflowClone
# This project aims to recreate the backend functionality of Stack Overflow.

> **Work in progress.** See [Roadmap](#roadmap) for the current state.

## About

A REST API inspired by Stack Overflow, built with ASP.NET Core and Entity Framework Core. The goal is to practice production-grade backend development: layered architecture, data integrity, testing and CI.

This project started as a practice exercise in an Entity Framework Core course. It has since been redesigned (Clean Architecture, soft delete, auditing) and extended well beyond the original scope.

*This project is not affiliated with Stack Overflow.*

## Features

**Foundation**
-  Domain model and database schema with EF Core migrations
-  Soft delete and automatic timestamps
-  API documentation (OpenAPI + Scalar)

**Planned**
-  Questions, answers and comments
-  Upvotes and downvotes
-  Tagging questions and filtering by tag
-  Accepting answers
-  Pagination and sorting
-  User registration, login (JWT) and account deletion

## Tech stack

- .NET 10, C# 14
- ASP.NET Core Web API (controllers)
- Entity Framework Core 10
- SQL Server (LocalDB for development)
- OpenAPI (`Microsoft.AspNetCore.OpenApi`) + Scalar

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

Detailed reasoning is recorded as Architecture Decision Records in [`docs/decisions`](docs/decisions).

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (Windows; installed with Visual Studio or SQL Server Express)

### Run locally

1. Clone the repository:
```
   git clone https://github.com/aDDOS12/StackOverflowClone.git
   cd StackOverflowClone
```
2. Create the database by applying migrations, using one of the options below.

   **Visual Studio (Package Manager Console)** – set *Default project* to `StackOverflowClone.Infrastructure`, then run:
```
   Update-Database
```
   **.NET CLI:**
```
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project src/StackOverflowClone.Infrastructure --startup-project src/StackOverflowClone.Api
```
3. Run the API:
```
   dotnet run --project src/StackOverflowClone.Api --launch-profile https
```
4. Open the API reference at `https://localhost:7266/scalar/v1`.

The connection string is configured in `src/StackOverflowClone.Api/appsettings.Development.json`.

If the browser warns about the certificate, trust the .NET development certificate once:
```
   dotnet dev-certs https --trust
```

## Roadmap

- [x] Phase 0 – Data model fixes, Clean Architecture structure, README
- [ ] Phase 1 – API foundation: first endpoints, error handling, validation
- [ ] Phase 2 – Questions, answers, comments, tags, pagination
- [ ] Phase 3 – Users and security: registration, JWT, authorization, account deletion (GDPR)
- [ ] Phase 4 – Business logic: voting, accepting answers
- [ ] Phase 5 – Unit and integration tests
- [ ] Phase 6 – Docker and GitHub Actions CI

## License

This project is licensed under the MIT License – see [LICENSE.txt](LICENSE.txt).
