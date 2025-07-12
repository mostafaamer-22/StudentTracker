using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Reports.DTOs;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Features.Reports.Queries.ExportStudents;

public sealed record ExportStudentsQuery(
    int? Grade = null,
    string? Subject = null,
    Guid? TeacherId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IQuery<IEnumerable<StudentExportDto>>;