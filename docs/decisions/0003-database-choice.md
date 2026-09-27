# 0003. Use SQL Server with LocalDB for development

- **Status:** Accepted
- **Date:** 2026-09-24

## Context

The project needs a relational database for local development. SQL Server Express and SQL Server Management Studio were previously uninstalled because SQL Server Express runs as a Windows service from system startup and uses memory even when not needed.
Job offers targeted by this portfolio mention both Microsoft SQL Server and PostgreSQL, with SQL Server appearing more often.
Docker is part of the project roadmap but had not been learned yet. No migrations existed at the time, so switching the database provider would have been cheap.

## Decision

We use Microsoft SQL Server as the database.

- **Development:** SQL Server LocalDB. It runs as a regular process that starts on demand when the application connects and stops automatically when idle.
- **Docker is postponed** to the DevOps phase, when a SQL Server container will be added together with the API container.
- The database is browsed with SQL Server Object Explorer in Visual Studio instead of SQL Server Management Studio.

## Consequences

+ No extra setup: the existing connection string and provider keep working.
+ Lightweight on Windows: no background service running when the project is not in use.
+ Matches the database most often required in targeted job offers.
- The development setup is Windows-only until Docker is introduced.
- Integration tests in CI (Linux runners) will need a different SQL Server instance, such as a container. This will be addressed in the DevOps phase.
- Some configuration is SQL Server–specific, e.g. the raw SQL in the comment check constraint uses T-SQL bracket syntax. Switching providers would require rewriting it and regenerating migrations.

## Alternatives considered

- **PostgreSQL in Docker** – lightweight and popular in newer projects, but requires Docker immediately and appears less often in targeted job offers.
- **SQL Server in Docker now** – cross-platform from the start, but learning Docker was postponed to keep the focus on the application code. To be revisited in the DevOps phase.
- **PostgreSQL installed natively** – runs as a Windows service, which is the problem that was being avoided.
- **SQLite** – no server needed, but it behaves differently from production databases and does not fit the planned Docker Compose setup.