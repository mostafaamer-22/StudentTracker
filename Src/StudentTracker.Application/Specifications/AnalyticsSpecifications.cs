using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Specification;

namespace StudentTracker.Application.Specifications;

public sealed class StudentsForAnalyticsSpecification : Specification<Student>
{
    public StudentsForAnalyticsSpecification(
        int? grade = null,
        string? subject = null,
        Guid? teacherId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        AddCriteria(x => x.IsActive);

        if (grade.HasValue)
        {
            AddCriteria(x => x.Grade == grade.Value);
        }

        if (teacherId.HasValue)
        {
            AddCriteria(x => x.StudentCourses.Any(sc =>
                sc.IsActive &&
                sc.Course.TeacherId == teacherId.Value));
        }

        if (!string.IsNullOrWhiteSpace(subject))
        {
            AddCriteria(x => x.StudentCourses.Any(sc =>
                sc.IsActive &&
                sc.Course.Name.ToLower().Contains(subject.ToLower())));
        }

        if (startDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc <= endDate.Value);
        }
    }
}

public sealed class StudentProgressForAnalyticsSpecification : Specification<StudentProgress>
{
    public StudentProgressForAnalyticsSpecification(
        List<Guid> studentIds,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        AddCriteria(x => studentIds.Contains(x.StudentId));

        if (startDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc <= endDate.Value);
        }

        AddInclude("Assignment");
    }
}

public sealed class AssessmentsForAnalyticsSpecification : Specification<Assessment>
{
    public AssessmentsForAnalyticsSpecification(
        List<Guid> studentIds,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        AddCriteria(x => studentIds.Contains(x.StudentId));

        if (startDate.HasValue)
        {
            AddCriteria(x => x.TakenAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            AddCriteria(x => x.TakenAt <= endDate.Value);
        }
    }
}

public sealed class AssignmentsForAnalyticsSpecification : Specification<Assignment>
{
    public AssignmentsForAnalyticsSpecification(
        List<Guid> courseIds,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        AddCriteria(x => x.IsActive);
        AddCriteria(x => courseIds.Contains(x.CourseId));

        if (startDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc <= endDate.Value);
        }
    }
}

public sealed class CoursesForAnalyticsSpecification : Specification<Course>
{
    public CoursesForAnalyticsSpecification(
        int? grade = null,
        string? subject = null,
        Guid? teacherId = null)
    {
        AddCriteria(x => x.IsActive);

        if (grade.HasValue)
        {
            AddCriteria(x => x.GradeLevel == grade.Value);
        }

        if (teacherId.HasValue)
        {
            AddCriteria(x => x.TeacherId == teacherId.Value);
        }

        if (!string.IsNullOrWhiteSpace(subject))
        {
            AddCriteria(x => x.Name.ToLower().Contains(subject.ToLower()) ||
                           x.Code.ToLower().Contains(subject.ToLower()));
        }
    }
}