using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Domain.Entities;

public class Teacher : User
{
    private Teacher() { }
    public string? Department { get; private set; }
    public string? Qualifications { get; private set; }
    public ICollection<Course> TeachingCourses { get; private set; } = [];
    public ICollection<Student> AssignedStudents { get; private set; } = [];
    public bool IsCurrentlyTeaching => TeachingCourses.Any(c => c.IsActive);
    public int ActiveStudentsCount => AssignedStudents.Count(s => s.IsActive);
    public int ActiveCoursesCount => TeachingCourses.Count(c => c.IsActive);

    public static Teacher Create(string firstName, string lastName, 
        string email, string? department = null, string? Qualifications = null)
    {
        return new Teacher
        {
            Email = email?.Trim().ToLowerInvariant() ?? string.Empty,
            UserName = email!.Split('@')[0],
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = (email!.Split('@')[0]).ToUpperInvariant(),
            FullName = $"{firstName} {lastName}".Trim(),
            Department = department?.Trim(),
            Qualifications = Qualifications?.Trim(),
            Id = Guid.NewGuid()
        };
    }


    public void AssignToCourse(Course course)
    {
        TeachingCourses.Add(course);
    }

    public void RemoveFromCourse(Course course)
    {
        var teachingCourse = TeachingCourses.FirstOrDefault(c => c.Id == course.Id);
        if (teachingCourse != null)
        {
            TeachingCourses.Remove(teachingCourse);
        }

    }

    public void AssignStudent(Student student)
    {
        AssignedStudents.Add(student);
    }

    public void UnassignStudent(Student student)
    {
        var assignedStudent = AssignedStudents.FirstOrDefault(s => s.Id == student.Id);
        if (assignedStudent != null)
        {
            AssignedStudents.Remove(assignedStudent);
        }

    }

    public IEnumerable<Student> GetStudentsInCourse(Guid courseId)
    {
        var course = TeachingCourses.FirstOrDefault(c => c.Id == courseId);
        if (course == null) return Enumerable.Empty<Student>();

        return course.StudentCourses
            .Where(sc => sc.IsActive)
            .Select(sc => sc.Student);
    }

    public decimal GetAverageStudentPerformance()
    {
        var allStudents = AssignedStudents.Where(s => s.IsActive).ToList();
        if (!allStudents.Any()) return 0;

        var averagePerformances = allStudents
            .Select(s => s.GetOverallCompletionPercentage())
            .Where(p => p > 0);

        return averagePerformances.Any() ? Math.Round(averagePerformances.Average(), 2) : 0;
    }

    public int GetTotalStudentsTeaching()
    {
        return TeachingCourses
            .SelectMany(c => c.StudentCourses)
            .Where(sc => sc.IsActive)
            .Select(sc => sc.StudentId)
            .Distinct()
            .Count();
    }

    public bool CanTeachGrade(int grade)
    {
        return TeachingCourses.Any(c => c.GradeLevel == grade && c.IsActive);
    }

    public bool CanTeachSubject(string subjectName)
    {
        if (string.IsNullOrWhiteSpace(subjectName)) return false;

        return TeachingCourses.Any(c =>
            c.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase) &&
            c.IsActive);
    }


}