using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StackOverflowClone.Domain.Common;

namespace StackOverflowClone.Infrastructure.Persistence.Interceptors;

public sealed class TimestampsAndSoftDeleteInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyRules(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.DeletedAt = now;
                continue;
            }

            if (entry.Entity is not IHasTimestamps timestamped)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                timestamped.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                timestamped.UpdatedAt = now;
            }
        }
    }
}
