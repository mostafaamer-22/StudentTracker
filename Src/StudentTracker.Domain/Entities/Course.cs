using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Domain.Entities;

public class Course : Entity<Guid>, IAuditableEntity
{
    private Course() { } 

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Code { get; private set; }
    public int GradeLevel { get; private set; }
    public decimal TotalPoints { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public Guid TeacherId { get; private set; }
    public Teacher Teacher { get; set; }
    public ICollection<Assignment> Assignments { get; private set; } = [];
    public ICollection<StudentCourse> StudentCourses { get; private set; } = [];
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public TimeSpan Duration => EndDate - StartDate;
    public bool IsCurrentlyActive => IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public bool HasStarted => DateTime.UtcNow >= StartDate;
    public bool HasEnded => DateTime.UtcNow > EndDate;
    public bool IsUpcoming => DateTime.UtcNow < StartDate;
    public int EnrolledStudentsCount => StudentCourses.Count(sc => sc.IsActive);
    public int TotalAssignmentsCount => Assignments.Count(a => a.IsActive);
    public decimal TotalAssignmentPoints => Assignments.Where(a => a.IsActive).Sum(a => a.MaxPoints);

    public static Course Create(string name,
        string code,
        string description,
        int gradeLevel,
        Guid teacherId,
        DateTime startDate,
        DateTime endDate, decimal totalPoints = 100)
    {
        return new Course
        {
            Name = name,
            Code = code.ToUpperInvariant(),
            Description = description,
            TeacherId = teacherId,
            GradeLevel = gradeLevel,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            TotalPoints = totalPoints,
            IsActive = true
        };
    }

    public void EnrollStudent(Student student)
    {
        var enrollment = StudentCourse.Create(student.Id, Id);
        StudentCourses.Add(enrollment);
    }


    public void AddAssignment(Assignment assignment)
    {
        Assignments.Add(assignment);
    }



    public decimal GetAverageStudentProgress()
    {
        var activeEnrollments = StudentCourses.Where(sc => sc.IsActive).ToList();
        if (!activeEnrollments.Any()) return 0;

        var progressPercentages = activeEnrollments
            .Select(sc => sc.Student.GetOverallCompletionPercentage())
            .Where(p => p > 0);

        return progressPercentages.Any() ? Math.Round(progressPercentages.Average(), 2) : 0;
    }

    public int GetCompletedAssignmentsCount()
    {
        return StudentCourses
            .Where(sc => sc.IsActive)
            .SelectMany(sc => sc.Student.ProgressRecords)
            .Count(pr => pr.Assignment.CourseId == Id && pr.IsCompleted);
    }

    public int GetTotalSubmissionsCount()
    {
        return StudentCourses
            .Where(sc => sc.IsActive)
            .SelectMany(sc => sc.Student.ProgressRecords)
            .Count(pr => pr.Assignment.CourseId == Id);
    }

    public IEnumerable<Student> GetActiveStudents()
    {
        return StudentCourses
            .Where(sc => sc.IsActive)
            .Select(sc => sc.Student)
            .Where(s => s.IsActive);
    }

    public IEnumerable<Assignment> GetActiveAssignments()
    {
        return Assignments.Where(a => a.IsActive);
    }

    public IEnumerable<Assignment> GetOverdueAssignments()
    {
        return Assignments.Where(a => a.IsActive && a.DueDate < DateTime.UtcNow);
    }

    public bool CanAcceptNewEnrollments()
    {
        return IsActive && !HasEnded;
    }

    public bool IsStudentEnrolled(Guid studentId)
    {
        return StudentCourses.Any(sc => sc.StudentId == studentId && sc.IsActive);
    }


}