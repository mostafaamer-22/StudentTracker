using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Students.DTOs;

namespace StudentTracker.Application.Features.Students.Queries.GetStudentById;

public sealed record GetStudentByIdQuery(Guid Id) : IQuery<StudentDetailsDto>;