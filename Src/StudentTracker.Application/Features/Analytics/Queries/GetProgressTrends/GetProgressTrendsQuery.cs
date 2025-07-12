using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Analytics.DTOs;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Features.Analytics.Queries.GetProgressTrends;

public sealed record GetProgressTrendsQuery(
    int? Grade = null,
    string? Subject = null,
    Guid? TeacherId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string Period = "weekly" // "daily", "weekly", "monthly"
) : IQuery<ProgressTrendsDto>;