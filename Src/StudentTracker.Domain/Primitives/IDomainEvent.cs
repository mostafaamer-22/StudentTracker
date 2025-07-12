using MediatR;

namespace StudentTracker.Domain.Primitives;
public interface IDomainEvent : INotification
{
    public Guid Id { get; init; }
}
