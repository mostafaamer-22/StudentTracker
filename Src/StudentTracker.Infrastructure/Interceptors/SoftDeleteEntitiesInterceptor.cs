using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using StudentTracker.Domain.Primitives;

namespace StudentTracker.Infrastructure.Interceptors;

public class SoftDeleteEntitiesInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<SoftDeleteEntitiesInterceptor> _logger;

    public SoftDeleteEntitiesInterceptor(ILogger<SoftDeleteEntitiesInterceptor> logger)
    {
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        return HandleChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        return HandleChangesAsync(eventData, result).GetAwaiter().GetResult();
    }

    private async ValueTask<InterceptionResult<int>> HandleChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
        {
            _logger.LogWarning("DbContext is null during SavingChanges.");
            return result;
        }

        var now = DateTime.UtcNow;

        var entries = dbContext
            .ChangeTracker
            .Entries<ISoftDeleteEntity>()
            .Where(e => e.State == EntityState.Deleted || e.Property(e => e.IsDeleted).IsModified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Deleted)
            {
                HandleSoftDelete(entityEntry, now);
            }
            else if (entityEntry.State == EntityState.Modified && entityEntry.Property(e => e.IsDeleted).IsModified)
            {
                if (entityEntry.Property(e => e.IsDeleted).CurrentValue == false)
                {
                    entityEntry.Property(e => e.RestoredAtUtc).CurrentValue = now;
                }
            }
        }

        return result;
    }

    private void HandleSoftDelete(EntityEntry<ISoftDeleteEntity> entityEntry, DateTime now)
    {
        entityEntry.State = EntityState.Modified;
        entityEntry.Property(nameof(ISoftDeleteEntity.IsDeleted)).CurrentValue = true;
        entityEntry.Property(nameof(ISoftDeleteEntity.DeletedAtUtc)).CurrentValue = now;

        HandleOwnedNavigations(entityEntry);
    }

    private void HandleOwnedNavigations(EntityEntry entityEntry)
    {
        var ownedNavigations = entityEntry.Metadata.GetNavigations()
            .Where(n => n.IsEagerLoaded);

        foreach (var ownedNavigation in ownedNavigations)
        {
            var ownedEntry = entityEntry.Reference(ownedNavigation.Name);
            if (ownedEntry?.TargetEntry?.State == EntityState.Deleted)
            {
                ownedEntry.TargetEntry.State = EntityState.Unchanged;
            }
        }
    }
}

