using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Students.DTOs;

namespace StudentTracker.Application.Features.Students.Queries.GetStudentProgress;

public sealed record GetStudentProgressQuery(
    Guid StudentId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IQuery<StudentProgressMetricsDto>;