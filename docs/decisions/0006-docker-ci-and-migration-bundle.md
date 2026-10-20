# 0006. Use Docker for tests and local runs, and apply migrations with a bundle

- **Status:** Accepted
- **Date:** 2026-10-20

## Context

ADR 0003 chose SQL Server LocalDB for development and postponed Docker. That setup is Windows-only, so it cannot run integration tests on the Linux runners used by GitHub Actions.
According to ADR 0004, services that query data are tested against a real database, so the tests need a real SQL Server instance, not an in-memory substitute.
The project is also a portfolio piece. A reviewer should be able to clone the repository and run the whole system with a single tool, without installing the .NET SDK, SQL Server or Visual Studio.
Once the API runs in a container, the database schema has to be created and updated there as well. This raises the question of who applies EF Core migrations, and when.

## Decision

- **Integration tests** start SQL Server 2022 in a Docker container with Testcontainers. One container is shared by all tests, the schema is created with `MigrateAsync()`, and Respawn resets the data before each test.
- **CI:** a GitHub Actions workflow builds the solution and runs all unit and integration tests on every push and pull request to `main`.
- **Dockerfile:** a multi-stage build. The .NET SDK image compiles the code; the final image contains only the ASP.NET Core runtime and the published application, and runs as a non-root user. The same Dockerfile has a separate `migrator` target.
- **Docker Compose** runs three services: `db` (SQL Server with a health check), `migrator` and `api`. Secrets are passed through a `.env` file that is excluded from Git; `.env.example` documents the required variables.
- **Migrations are applied by a separate container**, not by the API. The `migrator` image contains an EF Core migration bundle: a self-contained executable with all migrations. Compose starts it after the database is healthy, and starts the API only after the migrator exits successfully (`service_completed_successfully`).
- An `IDesignTimeDbContextFactory` creates the `DbContext` for EF Core tooling. Migrations therefore do not depend on the application host and its configuration (e.g. JWT settings). The factory reads the connection string from the same environment variable the API uses, with a LocalDB fallback for local development.
- **LocalDB remains** the default for day-to-day development in Visual Studio (ADR 0003).
- Compose runs the API in the `Development` environment to expose the API reference (Scalar). The Compose setup is meant for local runs and demos, not for production.

## Consequences

+ The whole system runs with a single command; Docker is the only prerequisite.
+ Tests, CI and Compose use the same SQL Server engine, so behaviour such as constraints, filtered indexes and transactions matches production.
+ The API never changes the database schema. A failed migration stops the startup chain instead of running the API against an incomplete schema.
+ The migrator image is based on the same runtime image as the API, so the base layers are shared.
- Docker is required to run integration tests, and starting the container makes a full test run slower.
- The default local connection string exists in two places: `appsettings.Development.json` and the design-time factory.
- Compose uses the `sa` login. A production setup would use separate logins with least privilege: one allowed to change the schema for migrations, and one limited to reading and writing data for the API.
- The Compose setup is not production-ready: secrets live in a local file, the API runs in `Development`, and there is no HTTPS inside the container.

## Alternatives considered

- **LocalDB for integration tests** – Windows-only, so tests could not run in Linux-based CI.
- **EF Core in-memory provider or SQLite for tests** – faster, but they behave differently from SQL Server (constraints, filtered unique indexes, transactions, raw T-SQL), which would hide bugs in exactly the logic the tests are meant to verify.
- **Applying migrations on application startup (`Database.Migrate()`)** – the simplest option. Since EF Core 9 a database lock prevents concurrent migrations from several instances, but the application would still need permission to change the schema, and a failed migration would block startup. Rejected to keep schema changes separate from the running application.
- **Manual `dotnet ef database update`** – requires the .NET SDK and the EF Core tool on the host, which breaks the goal of running the project with Docker only.
- **Idempotent SQL script (`dotnet ef migrations script --idempotent`) executed with `sqlcmd`** – produces SQL that can be reviewed before deployment, which suits environments where a DBA applies changes. More moving parts for a local setup, but a valid option for a production pipeline.