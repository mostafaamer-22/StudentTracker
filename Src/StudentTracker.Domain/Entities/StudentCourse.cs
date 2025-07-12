using StudentTracker.Domain.Primitives;

namespace StudentTracker.Domain.Entities;

public class StudentCourse : Entity<Guid>, IAuditableEntity
{
    public DateTime EnrollmentDate { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid StudentId { get;  private set; }
    public Guid CourseId { get;  private set; }
    public Student Student { get; private set; } = null!;
    public Course Course { get; private set; } = null!;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }

    private StudentCourse() { }

    public static StudentCourse Create(Guid studentId, Guid courseId)
    {
        return new StudentCourse
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow
        };
    }
}