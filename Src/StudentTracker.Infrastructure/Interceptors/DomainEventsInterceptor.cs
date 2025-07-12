using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using StudentTracker.Domain.Primitives;

namespace StudentTracker.Infrastructure.Interceptors;

public class DomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<DomainEventsInterceptor> _logger;
    private readonly IPublisher _publisher;

    public DomainEventsInterceptor(ILogger<DomainEventsInterceptor> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
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

        var domainEvents = new List<IDomainEvent>();
        var aggregateRoots = dbContext.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .Where(entity => entity.GetType().BaseType?.IsGenericType == true
                             && entity.GetType().BaseType!.GetGenericTypeDefinition() == typeof(AggregateRoot<>));

        foreach (var aggregateRoot in aggregateRoots)
        {
            var getDomainEventsMethod = aggregateRoot.GetType().GetMethod("GetDomainEvents");
            var clearDomainEventsMethod = aggregateRoot.GetType().GetMethod("ClearDomainEvents");

            if (getDomainEventsMethod is null || clearDomainEventsMethod is null)
                continue;

            if (getDomainEventsMethod.Invoke(aggregateRoot, null) is List<IDomainEvent> events)
            {
                domainEvents.AddRange(events);
                clearDomainEventsMethod.Invoke(aggregateRoot, null);
            }
        }

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}

