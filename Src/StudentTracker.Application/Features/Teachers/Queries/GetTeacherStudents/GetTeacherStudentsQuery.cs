using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Students.DTOs;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Features.Teachers.Queries.GetTeacherStudents;

public record GetTeacherStudentsQuery(
    Guid TeacherId,
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null) : IPaginateQuery<GetStudentsForSpecificalTeacherDto>;