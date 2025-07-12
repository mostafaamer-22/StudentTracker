using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class Assessment : Entity<Guid>, IAuditableEntity
{
    private Assessment() { }
    public decimal Score { get; private set; }
    public decimal MaxScore { get; private set; }
    public AssessmentType Type { get; private set; }
    public DateTime TakenAt { get; private set; }
    public int TimeSpentMinutes { get; private set; }
    public int AttemptNumber { get; private set; }
    public string? Feedback { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public Assignment Assignment { get; private set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public decimal PercentageScore => MaxScore > 0 ? (Score / MaxScore) * 100 : 0;
    public bool IsPassing => PercentageScore >= 70;

    public static Assessment Create(decimal score,
        decimal maxScore,
        AssessmentType type,
        DateTime takenAt,
        int timeSpentMinutes,
        int attemptNumber,
        Guid studentId,
        Guid assignmentId,
        string? feedback = null)
    {
        return new Assessment
        {
            Id = Guid.NewGuid(),
            Score = score,
            MaxScore = maxScore,
            Type = type,
            TakenAt = takenAt,
            TimeSpentMinutes = timeSpentMinutes,
            AttemptNumber = attemptNumber,
            StudentId = studentId,
            AssignmentId = assignmentId,
            Feedback = feedback?.Trim(),
        };
    }

}