using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class Student : User
{
    private Student() : base() { }
    public int Grade { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string? ParentEmail { get; private set; }
    public ICollection<StudentProgress> ProgressRecords { get; private set; } = [];
    public ICollection<Assessment> Assessments { get; private set; } = [];
    public ICollection<StudentCourse> StudentCourses { get; private set; } = [];

    public int Age => DateTime.Today.Year - DateOfBirth.Year -
        (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public bool IsMinor => Age < 18;
    public string GradeDisplay => Grade switch
    {
        0 => "Kindergarten",
        var g when g >= 1 && g <= 12 => $"Grade {g}",
        _ => "Unknown"
    };

    public static Student Create(string firstName,
        string lastName,
        string email,
        int grade, DateTime dateOfBirth,
        string? parentEmail = null)
    {
        return new Student
        {
            Email = email?.Trim().ToLowerInvariant() ?? string.Empty,
            NormalizedEmail = email?.Trim().ToUpperInvariant(),
            FullName = $"{firstName} {lastName}".Trim(),
            Grade = grade,
            DateOfBirth = dateOfBirth.Date,
            ParentEmail = parentEmail?.Trim(),
            Id = Guid.NewGuid()
        };
    }


    public void SetGrade(int grade)
    {
        Grade = grade;

    }

    public void SetDateOfBirth(DateTime dateOfBirth)
    {
        DateOfBirth = dateOfBirth.Date;
    }


    public void EnrollInCourse(Course course)
    {
        var enrollment = StudentCourse.Create(Id, course.Id);
        StudentCourses.Add(enrollment);
    }

    public decimal GetOverallCompletionPercentage()
    {
        if (!ProgressRecords.Any()) return 0;
        return Math.Round(ProgressRecords.Average(p => p.CompletionPercentage), 2);
    }

    public int GetCompletedAssignmentsCount()
    {
        return ProgressRecords.Count(p => p.Status == ProgressStatus.Completed);
    }

    public int GetTotalAssignmentsCount()
    {
        return ProgressRecords.Count;
    }

    public decimal GetAverageAssessmentScore()
    {
        if (!Assessments.Any()) return 0;
        return Math.Round(Assessments.Average(a => a.PercentageScore), 2);
    }

    public IEnumerable<Assessment> GetRecentAssessments(int count = 5)
    {
        return Assessments.OrderByDescending(a => a.TakenAt).Take(count);
    }

    public bool IsActiveInCourse(Guid courseId)
    {
        return StudentCourses.Any(sc => sc.CourseId == courseId && sc.IsActive);
    }

    public bool HasOverdueAssignments()
    {
        return ProgressRecords.Any(p => p.IsOverdue);
    }

    public IEnumerable<StudentProgress> GetOverdueAssignments()
    {
        return ProgressRecords.Where(p => p.IsOverdue);
    }


}