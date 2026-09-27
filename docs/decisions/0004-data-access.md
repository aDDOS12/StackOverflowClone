# 0004. Access data through an application-level DbContext interface

- **Status:** Accepted
- **Date:** 2026-09-27

## Context

Application logic needs to read and save data, but according to ADR 0002 the Application project cannot reference Infrastructure, where the EF Core `DbContext` lives.
EF Core already implements the Repository and Unit of Work patterns: each `DbSet<T>` acts as a repository, and `DbContext` tracks changes and saves them in a single transaction.

## Decision

- Application defines `IApplicationDbContext`, which exposes the entity sets (`DbSet<T>`) and `SaveChangesAsync`.
- `StackOverflowContext` in Infrastructure implements this interface. It is registered in DI as a scoped service that resolves to the same `StackOverflowContext` instance used by the rest of the request.
- Application services use LINQ on this interface directly, including `Include`, projections to DTOs and `AsNoTracking`.
- Application references `Microsoft.EntityFrameworkCore` (for `DbSet<T>`), but not any database provider.
- Constants used by queries, such as query filter names, live in Application.

## Consequences

+ No duplicated abstraction on top of EF Core, and less code.
+ Full LINQ power in application services: each query can be shaped exactly for its use case.
+ The database provider stays isolated in Infrastructure.
- Application depends on EF Core. This is a deliberate relaxation of Clean Architecture.
- `DbSet<T>` is hard to mock, so services that query data are tested against a real database (integration tests) rather than with mocks.
- Query logic lives in services, so repeated queries must be extracted deliberately to avoid duplication.

## Alternatives considered

- **Specific repositories with `IUnitOfWork`** – keeps Application free of EF Core, simplifies mocking and would be a good exercise in both patterns. Rejected because it duplicates what `DbSet` and `DbContext` already provide and limits queries to predefined methods.
- **Generic `IRepository<T>`** – duplicates `DbSet` and either leaks `IQueryable` or restricts queries.
- **Referencing Infrastructure from Application** – violates the dependency rule from ADR 0002.