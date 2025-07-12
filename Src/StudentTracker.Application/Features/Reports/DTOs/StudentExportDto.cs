namespace StudentTracker.Application.Features.Reports.DTOs;

public class StudentExportDto
{
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Grade { get; set; }
    public string DateOfBirth { get; set; } = string.Empty;
    public string EnrollmentDate { get; set; } = string.Empty;
    public string? ParentEmail { get; set; }
    public string IsActive { get; set; } = string.Empty;
    public string OverallCompletionPercentage { get; set; } = string.Empty;
    public string AverageScore { get; set; } = string.Empty;
    public string TotalAssignments { get; set; } = string.Empty;
    public string CompletedAssignments { get; set; } = string.Empty;
    public string OverdueAssignments { get; set; } = string.Empty;
    public string TotalTimeSpentHours { get; set; } = string.Empty;
    public string LastActivity { get; set; } = string.Empty;
    public string Courses { get; set; } = string.Empty;
    public string Teachers { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string LastModified { get; set; } = string.Empty;
}