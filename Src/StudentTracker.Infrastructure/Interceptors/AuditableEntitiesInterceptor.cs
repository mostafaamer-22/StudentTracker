using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using StudentTracker.Domain.Primitives;

namespace StudentTracker.Infrastructure.Interceptors;

public class AuditableEntitiesInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<AuditableEntitiesInterceptor> _logger;

    public AuditableEntitiesInterceptor(ILogger<AuditableEntitiesInterceptor> logger)
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
            .Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property("CreatedOnUtc").CurrentValue = now;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                if (HasModifiedProperties(entityEntry))
                {
                    entityEntry.Property("ModifiedOnUtc").CurrentValue = now;
                }
            }
        }

        return result;
    }

    private static bool HasModifiedProperties(EntityEntry entityEntry)
    {
        return entityEntry.Properties.Any(p => p.IsModified && p.Metadata.Name != "CreatedOnUtc");
    }
}

