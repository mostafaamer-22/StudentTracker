namespace StudentTracker.Application.Features.Students.DTOs;

public sealed record StudentListDto(
    Guid Id,
    string FullName,
    string Email,
    int Grade,
    string GradeDisplay,
    int Age,
    bool IsMinor,
    decimal OverallCompletionPercentage,
    int CompletedAssignments,
    int TotalAssignments,
    decimal AverageAssessmentScore);

public sealed record SearchStudentDto(
    Guid Id,
    string FullName,
    string Email,
    int Grade,
    string GradeDisplay,
    int Age,
    bool IsMinor);

public sealed record StudentDetailsDto(
    Guid Id,
    string FullName,
    string Email,
    int Grade,
    string GradeDisplay,
    int Age,
    bool IsMinor,
    string ParentEmail,
    DateTime DateOfBirth,
    decimal OverallCompletionPercentage,
    int CompletedAssignments,
    int TotalAssignments,
    decimal AverageAssessmentScore,
    IReadOnlyList<StudentProgressDto> RecentProgress,
    IReadOnlyList<StudentAssessmentDto> RecentAssessments,
    IReadOnlyList<StudentCourseDto> ActiveCourses);

public sealed record StudentProgressDto(
    Guid Id,
    string AssignmentName,
    decimal CompletionPercentage,
    int TimeSpentMinutes,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt);

public sealed record StudentAssessmentDto(
    Guid Id,
    string Type,
    decimal PercentageScore,
    DateTime TakenAt);

public sealed record StudentCourseDto(
    Guid Id,
    string Name,
    string Code,
    DateTime EnrollmentDate,
    DateTime? CompletionDate,
    bool IsActive);