# 0002. Use Clean Architecture with separate projects

- **Status:** Accepted
- **Date:** 2026-09-26

## Context

The current scope of this project is small: a handful of entities and no business logic yet. At this size, a single project with feature folders would be sufficient.
However, this project also server as a learning exercise and a portfolio piece. Clean Architecture is widely used in commercial .NET projects, and the planned features (authentication, voting logic, tests) will expand the codebase in significant manner.
The original single project mixed entities, EF Core configuration and API setup. Nothing prevented, for example, domain entities from depending on EF Core.

## Decision

We use Clean Architecture with four projects:

- **Domain** - entities and core abstractions (`ISoftDeletable`, `IHasTimestamps`). No dependencies on other projects or NuGet packages.
- **Application** - use cases, DTOs and validation. Depends only on Domain.
- **Infrastructure** - EF Core: `DbContext`, migrations, interceptors, seed data. Depends on Application.
- **Api** - controllers and the composition root. Depends on Application and Infrastructure.

Dependencies point inwards and are enforced by project references. Each outer layer exposes registration method (e.g. `AddInfrastructure`) called from `Program.cs`, so the Api does not configure infrastructure details itself.
Controllers depend only on Application. Because project references are transitive, the Api can technically see Domain and Infrastructure types, so this rule is a convention checked in code review.
We avoid additional patterns (MediatR, CQRS, generic repositories) until a concrete problem requires them.

## Consequences

+ The compiler enforces the inner boundaries: Domain cannot reference EF Core, and Application cannot reference Infrastructure.
+ The database technology is isolated in Infrastructure. Switching from SQL Server to another database affects only this project.
+ Application logic can be unit tested without a database or HTTP.
+ The structure matches what many commercial .NET codebases use.
- ➖ More ceremony for a small project: four projects, extra references and more files to navigate.
- ➖ A single feature is spread across several projects.
- ➖ Some rules, such as controllers using only Application, are not enforced by the compiler.
- ➖ Application needs an abstraction to access data without referencing Infrastructure. This will be decided in a separate ADR.

## Alternatives considered

- **Folder-by-type in a single project** (`Controllers/`, `Services/`, `Entities/`) – the simplest option, but a single feature is scattered across folders and no boundaries are enforced.
- **Feature folders (vertical slices) in a single project** – a good fit for the current size that scales well, but layer boundaries rely only on discipline. Rejected mainly to practice the layered approach common in commercial .NET projects.
 