namespace StudentTracker.Application.Features.Students.Queries.GetStudents;

public class StudentFilterModel
{
    public StudentFilterModel()
    {
        Page = 1;
        PageSize = 10;
        SortDesc = false;
    }

    public string? SearchTerm { get; set; }
    public int? Grade { get; set; }
    public string? CourseName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public static readonly string[] AllowedSortFields =
    {
        "fullName",
        "grade",
        "overallProgress",
        "lastActivity",
        "assessmentScore"
    };
}