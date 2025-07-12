using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Analytics.DTOs;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Features.Analytics.Queries.GetClassSummary;

public sealed record GetClassSummaryQuery(
    int? Grade = null,
    string? Subject = null,
    Guid? TeacherId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IQuery<ClassSummaryDto>;