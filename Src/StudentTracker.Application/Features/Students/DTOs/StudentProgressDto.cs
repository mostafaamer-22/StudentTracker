using StudentTracker.Domain.Enums;

namespace StudentTracker.Application.Features.Students.DTOs;

public sealed record StudentProgressMetricsDto(
    decimal OverallCompletionPercentage,
    int CompletedAssignments,
    int TotalAssignments,
    decimal AverageAssessmentScore,
    IReadOnlyList<CourseProgressDto> SubjectProgress,
    IReadOnlyList<MonthlyProgressDto> MonthlyProgress,
    IReadOnlyList<RecentActivityDto> RecentActivities);

public sealed record CourseProgressDto(
    string CourseName,
    decimal CompletionPercentage,
    int CompletedAssignments,
    int TotalAssignments,
    decimal AverageScore);

public sealed record MonthlyProgressDto(
    DateTime Month,
    decimal CompletionPercentage,
    int AssignmentsCompleted,
    decimal AverageScore);

public sealed record RecentActivityDto(
    Guid Id,
    string Title,
    ActivityType Type,
    DateTime Timestamp,
    decimal? Score,
    decimal? CompletionPercentage,
    string Status);

public sealed record UpdateProgressRequestDto(
    Guid AssignmentId,
    decimal CompletionPercentage,
    int TimeSpentMinutes,
    ProgressStatus Status,
    string? Notes);