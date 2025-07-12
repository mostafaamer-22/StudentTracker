using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Students.DTOs;

namespace StudentTracker.Application.Features.Students.Queries.GetStudents;

public sealed class GetStudentsQuery : StudentFilterModel, IPaginateQuery<SearchStudentDto>
{
    public GetStudentsQuery()
    {
        Page = 1;
        PageSize = 10;
    }

    public GetStudentsQuery(
        string? searchTerm = null,
        int? grade = null,
        string? courseName = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? sortBy = null,
        bool sortDesc = false,
        int page = 1,
        int pageSize = 10) : base()
    {
        SearchTerm = searchTerm;
        Grade = grade;
        CourseName = courseName;
        StartDate = startDate;
        EndDate = endDate;
        SortBy = sortBy;
        SortDesc = sortDesc;
        Page = page <= 0 ? 1 : page;
        PageSize = pageSize <= 0 ? 10 : pageSize;
    }
}