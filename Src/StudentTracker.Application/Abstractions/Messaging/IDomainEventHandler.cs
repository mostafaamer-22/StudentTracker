using MediatR;
using StudentTracker.Domain.Primitives;

namespace Application.Abstractions.Messaging;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
