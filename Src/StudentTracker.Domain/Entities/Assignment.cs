using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class Assignment : Entity<Guid>, IAuditableEntity
{
    private Assignment() { }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AssignmentType Type { get; private set; }
    public decimal MaxPoints { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? OpenDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int EstimatedMinutes { get; private set; }
    public string? Instructions { get; private set; }
    public Guid CourseId { get; private set; }
    public Course Course { get; private set; } = null!;
    public ICollection<StudentProgress> StudentProgress { get; private set; } = [];
    public ICollection<Assessment> Assessments { get; private set; } = [];
    public DateTime CreatedOnUtc { get;  set; }
    public DateTime? ModifiedOnUtc { get; set; }

    public static Assignment Create(string title,
        string description,
        AssignmentType type,
        decimal maxPoints,
        DateTime dueDate,
        int estimatedMinutes,
        Guid courseId,
        DateTime? openDate = null,
        string? instructions = null)
    {
        return new Assignment
        {
            Title = title.Trim(),
            Description = description.Trim(),
            Type = type,
            MaxPoints = maxPoints,
            DueDate = dueDate.Date,
            OpenDate = openDate?.Date,
            EstimatedMinutes = estimatedMinutes,
            Instructions = instructions?.Trim(),
            CourseId = courseId
        };
    }
}