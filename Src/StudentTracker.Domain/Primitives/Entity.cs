namespace StudentTracker.Domain.Primitives;

public abstract class Entity<TKey>
{
    protected Entity(TKey id) => Id = id;
    protected Entity() { }
    public TKey Id { get; init; }
}
