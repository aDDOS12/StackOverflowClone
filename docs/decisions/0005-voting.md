# 0005. Store votes per user and update scores atomically

- **Status:** Accepted
- **Date:** 2026-10-20

## Context

Users can upvote and downvote questions and answers. Each user may have at most one vote per item, and may change or remove it.
Lists of questions and answers can be sorted by score, so the score must be cheap to read.
Popular posts can receive many votes at the same time. A naive read-modify-write of the score (load the entity, change `Score`, save) loses updates when two requests run concurrently.
HTTP clients and proxies may retry requests, so repeating a vote request must not change the score twice.
This decision was recorded after the implementation.

## Decision

- **One row per vote:** `QuestionVote` and `AnswerVote` store `UserId`, the target id and `Value` (`+1` or `-1`). A composite primary key (`UserId`, target id) guarantees at most one vote per user and item at the database level.
- **Denormalized score:** `Question` and `Answer` keep a `Score` column (`IVoteable`), so sorting and paging by score do not aggregate votes on every read.
- **Idempotent endpoints:** `PUT .../vote` with `{ "value": 1 | -1 }` sets the user's vote, and `DELETE .../vote` removes it. Repeating the same request leaves the state unchanged.
- **Vote transitions in Domain:** `VoteChange.Calculate(currentValue, newValue)` is a pure function that returns the action (add, change, remove, none) and the score delta, e.g. `+1 → -1` gives `-2`. When the delta is `0`, the service returns early without touching the database.
- **Atomic score update:** the score is changed with `ExecuteUpdateAsync`, which runs a single SQL statement `UPDATE ... SET Score = Score + @delta`. The vote row change and the score update run in one explicit transaction.
- `ExecuteUpdateAsync` bypasses the change tracker and the `SaveChangesInterceptor`, so voting does not set `UpdatedAt`. Voting is not an edit of the content.

## Consequences

+ No lost updates under concurrent voting, without optimistic concurrency or retry logic. An integration test sends concurrent votes and checks the final score.
+ Retried requests are safe, because the endpoints are idempotent.
+ Vote transitions are covered by fast unit tests without a database.
+ Reading and sorting by score is a simple column read.
- The score is stored twice: as the sum of votes and as the `Score` column. Any code that changes votes outside the voting service must keep them in sync.
- `ExecuteUpdateAsync` skips the interceptor and change tracker. Entities already loaded in the same `DbContext` do not see the new score.
- Votes on comments exist in the data model but are not exposed through the API yet.

## Alternatives considered

- **Separate `POST .../upvote` and `.../downvote` endpoints with toggle semantics** – a retried request would flip the vote back, so they are not idempotent.
- **Computing the score with `SUM` over votes on every read** – always consistent, but expensive for lists sorted and paged by score.
- **Read-modify-write through the change tracker** – the simplest code, but loses updates when two votes arrive at the same time.
- **Optimistic concurrency with a `rowversion` column** – correct, but requires retry logic and causes conflicts on popular posts, which is where concurrent votes happen most.