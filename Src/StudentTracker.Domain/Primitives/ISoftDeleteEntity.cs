namespace StudentTracker.Domain.Primitives;
public interface ISoftDeleteEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; }
    public DateTime? RestoredAtUtc { get; }
}