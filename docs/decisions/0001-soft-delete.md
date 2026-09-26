# 0001. Use soft delete for user content and accounts

- **Status:** Accepted
- **Date:** 2026-09-25

## Context

Questions, answers and comments form threads that many users contribute to. Deleting one user's account must not destory content written by others, and moderators should be able to restore deleted posts, as on Stack Overflow.
Had deletes with cascades are also not possible in this schema: SQL Server rejects multiple cascade paths to the same table (e.g. `Users -> Comments` and `Questions -> Comments`).
At the same time, GDPR requires removing a user's personal data on request.

## Decision

- Question, answers, comments and users are never physically deleted. A nullable `DeletedAt` timestamp marks a record as deleted. There is no separate 'IsDeleted' flag, so the state has a single source of truth.
- A `SaveChangesInterceptor` turns every delete of an `ISoftDeletable` entity into an update that sets `DeletedAt`. Application code keeps using the natural `Remove()` call.
- Named global query filters (`SoftDelete`) hide deleted records. Votes and question-tag links are filtered through their parent entity.
- Users have **no** query filter. Required navigations use `INNER JOIN`, so a filter on users would silently hide all content written by deleted users.
- All foreign keys from soft-deletable entities use `Restrict`, so the database blocks accidental hard deletes.
- Deleting an account will anonymize personal data (username, email, password hash) instead of removing the row.

## Consequences

+ Threads stay intact, and deleted content can be restored.
+ No Cascade path conflicts. The database protects against accidental hard deletes.
- Queries depend on global filters. Bypassing them (`IgnoreQueryFilters`) must be deliberate.
- Soft delete does not cascade. Answers of a deleted question are hidden when accessed through the question, but a direct query on asnwers must handle this case explicitly.
- Deleted rows remain in the tables, so unique indexes (e.g. usernames) must account for them.

## Alternatives considered

- **Hard delete with cascades** – destroys other users' content and is rejected by SQL Server because of multiple cascade paths.
- **Hard delete of users, with `UserId` set to `null` on their content** – forces handling of a missing author everywhere and does not cover deleting content itself.
- **`IsDeleted` and `DeletedAt` together** – two fields can contradict each other (`IsDeleted = true`, `DeletedAt = null`).