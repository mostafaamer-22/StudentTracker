using StudentTracker.Application.Features.Students.Queries.GetStudents;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Domain.Specification;

public sealed class StudentFilterSpecification : Specification<Student>
{
    public StudentFilterSpecification(GetStudentsQuery query)
    {
        AddCriteria(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            AddCriteria(x => x.FullName.ToLower().Contains(searchTerm) ||
                    x.Email.ToLower().Contains(searchTerm) ||
                    (x.ParentEmail != null && x.ParentEmail.ToLower().Contains(searchTerm)));
        }

        if (query.Grade.HasValue)
        {
            AddCriteria(x => x.Grade == query.Grade.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.CourseName))
        {
            AddCriteria(x => x.StudentCourses.Any(sc =>
                sc.IsActive &&
                sc.Course.Name.ToLower() == query.CourseName.ToLower()));
        }

        if (query.StartDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            AddCriteria(x => x.CreatedOnUtc <= query.EndDate.Value);
        }


        if (!string.IsNullOrWhiteSpace(query.SortBy) &&
            StudentFilterModel.AllowedSortFields.Contains(query.SortBy.ToLower()))
        {
            switch (query.SortBy.ToLower())
            {
                case "fullname":
                    AddOrderBy(x => x.FullName, query.SortDesc);
                    break;
                case "grade":
                    AddOrderBy(x => x.Grade, query.SortDesc);
                    break;
                case "overallprogress":
                case "lastactivity":
                case "assessmentscore":
                    AddOrderBy(x => x.FullName, false);
                    break;
                default:
                    AddOrderBy(x => x.FullName, false);
                    break;
            }
        }
        else
        {
            AddOrderBy(x => x.FullName, false);
        }

        ApplyPaging(query.PageSize, query.Page);
    }
}

public sealed class StudentByIdSpecification : Specification<Student>
{
    public StudentByIdSpecification(Guid id, bool includeDetails = false)
    {
        AddCriteria(x => x.Id == id);
        AddCriteria(x => x.IsActive);

        if (includeDetails)
        {
            AddInclude("Assessments");
            AddInclude("StudentCourses.Course");
            AddInclude("ProgressRecords.Assignment");
        }
    }
}

// New specification for getting students by teacher ID
public sealed class StudentsByTeacherSpecification : Specification<Student>
{
    public StudentsByTeacherSpecification(Guid teacherId, int page = 1, int pageSize = 20, string? searchTerm = null)
    {
        AddCriteria(x => x.IsActive);

        AddCriteria(x => x.StudentCourses.Any(sc =>
            sc.IsActive &&
            sc.Course.TeacherId == teacherId &&
            sc.Course.IsActive));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            AddCriteria(x => x.FullName.ToLower().Contains(searchTermLower) ||
                           x.Email.ToLower().Contains(searchTermLower) ||
                           (x.ParentEmail != null && x.ParentEmail.ToLower().Contains(searchTermLower)));
        }

        AddOrderBy(x => x.FullName, false);

        ApplyPaging(pageSize, page);
    }
}