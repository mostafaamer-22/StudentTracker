using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Specification;

namespace StudentTracker.Domain.Specification;

// Basic student specification without heavy includes
public sealed class StudentBasicSpecification : Specification<Student>
{
    public StudentBasicSpecification(Guid studentId)
    {
        AddCriteria(x => x.Id == studentId && x.IsActive);
        AddInclude("StudentCourses.Course");
    }
}

// Specification for student progress records with date filtering
public sealed class StudentProgressRecordsSpecification : Specification<StudentProgress>
{
    public StudentProgressRecordsSpecification(Guid studentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        AddCriteria(x => x.StudentId == studentId);

        if (startDate.HasValue)
            AddCriteria(x => x.StartedAt >= startDate.Value);

        if (endDate.HasValue)
            AddCriteria(x => x.StartedAt <= endDate.Value);

        AddInclude("Assignment");
        AddOrderByDescending(x => x.LastAccessedAt);
    }
}

// Specification for student assessments with date filtering
public sealed class StudentAssessmentsSpecification : Specification<Assessment>
{
    public StudentAssessmentsSpecification(Guid studentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        AddCriteria(x => x.StudentId == studentId);

        if (startDate.HasValue)
            AddCriteria(x => x.TakenAt >= startDate.Value);

        if (endDate.HasValue)
            AddCriteria(x => x.TakenAt <= endDate.Value);

        AddInclude("Assignment");
        AddOrderByDescending(x => x.TakenAt);
    }
}

// Specification for course assignments for a specific student
public sealed class StudentCourseAssignmentsSpecification : Specification<Assignment>
{
    public StudentCourseAssignmentsSpecification(Guid studentId)
    {
        AddCriteria(x => x.Course.StudentCourses.Any(sc => sc.StudentId == studentId && sc.IsActive));
    }
}