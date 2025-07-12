using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class StudentProgress : Entity<Guid>, IAuditableEntity
{
    private StudentProgress() { }
    public decimal CompletionPercentage { get; private set; }
    public int TimeSpentMinutes { get; private set; }
    public ProgressStatus Status { get; private set; }
    public decimal? EarnedPoints { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime LastAccessedAt { get; private set; }
    public int AccessCount { get; private set; }
    public string? Notes { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public Assignment Assignment { get; private set; } = null!;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public bool IsCompleted => Status == ProgressStatus.Completed;
    public bool IsOverdue => Assignment.DueDate < DateTime.UtcNow && !IsCompleted;

    public static StudentProgress Create(decimal completionPercentage,
        int timeSpentMinutes,
        ProgressStatus status,
        decimal? earnedPoints,
        DateTime? startedAt,
        DateTime? completedAt,
        DateTime lastAccessedAt,
        int accessCount,
        string? notes,
        Guid studentId,
        Guid assignmentId)
    {
        return new StudentProgress
        {
            CompletionPercentage = completionPercentage,
            TimeSpentMinutes = timeSpentMinutes,
            Status = status,
            EarnedPoints = earnedPoints,
            StartedAt = startedAt.HasValue && startedAt.Value.Kind == DateTimeKind.Utc ?
            startedAt.Value : startedAt?.ToUniversalTime(),
            CompletedAt = completedAt.HasValue && completedAt.Value.Kind == DateTimeKind.Utc ?
            completedAt.Value : completedAt?.ToUniversalTime(),
            LastAccessedAt = lastAccessedAt.ToUniversalTime(),
            AccessCount = accessCount,
            Notes = notes?.Trim(),
            StudentId = studentId,
            AssignmentId = assignmentId
        };
    }

    public void UpdateProgress(
        decimal completionPercentage,
        int timeSpentMinutes,
        ProgressStatus status,
        decimal? earnedPoints,
        DateTime? startedAt,
        DateTime? completedAt,
        DateTime lastAccessedAt,
        int accessCount,
        string? notes)
    {
        CompletionPercentage = completionPercentage;
        TimeSpentMinutes = timeSpentMinutes;
        Status = status;
        EarnedPoints = earnedPoints;
        StartedAt = startedAt.HasValue && startedAt.Value.Kind == DateTimeKind.Utc ?
            startedAt.Value : startedAt?.ToUniversalTime();
        CompletedAt = completedAt.HasValue && completedAt.Value.Kind == DateTimeKind.Utc ?
            completedAt.Value : completedAt?.ToUniversalTime();
        LastAccessedAt = lastAccessedAt.ToUniversalTime();
        AccessCount = accessCount;
        Notes = notes?.Trim();
        ModifiedOnUtc = DateTime.UtcNow;
    }
}