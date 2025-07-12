using StudentTracker.Domain.Primitives;

namespace StudentTracker.Domain.DomainEvents;
public abstract record DomainEvent(Guid Id) : IDomainEvent;
