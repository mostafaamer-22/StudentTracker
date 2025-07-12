using StudentTracker.Application.Abstractions.Messaging;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Application.Features.Students.Commands.UpdateStudentProgress;

public sealed record UpdateStudentProgressCommand(
    Guid StudentId,
    Guid AssignmentId,
    decimal CompletionPercentage,
    int TimeSpentMinutes,
    ProgressStatus Status,
    decimal? EarnedPoints,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime LastAccessedAt,
    int AccessCount,
    string? Notes) : ICommand;