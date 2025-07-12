using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Specification;

namespace StudentTracker.Application.Specifications;

public sealed class StudentsForExportSpecification : Specification<Student>
{
    public StudentsForExportSpecification(
        int? grade = null,
        string? subject = null,
        Guid? teacherId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null)
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

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            AddCriteria(x => x.FullName.ToLower().Contains(searchLower) ||
                           x.Email.ToLower().Contains(searchLower) ||
                           (x.ParentEmail != null && x.ParentEmail.ToLower().Contains(searchLower)));
        }

        AddOrderBy(x => x.FullName, false);
    }
}

public sealed class StudentProgressForExportSpecification : Specification<StudentProgress>
{
    public StudentProgressForExportSpecification(List<Guid> studentIds)
    {
        AddCriteria(x => studentIds.Contains(x.StudentId));

        // Include Assignment for IsOverdue calculation
        AddInclude("Assignment");
    }
}

public sealed class AssessmentsForExportSpecification : Specification<Assessment>
{
    public AssessmentsForExportSpecification(List<Guid> studentIds)
    {
        AddCriteria(x => studentIds.Contains(x.StudentId));
    }
}

public sealed class StudentCoursesForExportSpecification : Specification<StudentCourse>
{
    public StudentCoursesForExportSpecification(List<Guid> studentIds)
    {
        AddCriteria(x => studentIds.Contains(x.StudentId));
        AddCriteria(x => x.IsActive);
    }
}

public sealed class CoursesForExportSpecification : Specification<Course>
{
    public CoursesForExportSpecification(List<Guid> courseIds)
    {
        AddCriteria(x => courseIds.Contains(x.Id));
        AddCriteria(x => x.IsActive);
    }
}

public sealed class TeachersForExportSpecification : Specification<Teacher>
{
    public TeachersForExportSpecification(List<Guid> teacherIds)
    {
        AddCriteria(x => teacherIds.Contains(x.Id));
        AddCriteria(x => x.IsActive);
    }
}