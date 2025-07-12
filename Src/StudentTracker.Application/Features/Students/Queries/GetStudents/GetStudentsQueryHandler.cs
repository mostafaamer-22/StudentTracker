using Application.Abstractions.Messaging;
using Mapster;
using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Features.Students.DTOs;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Specification;

namespace StudentTracker.Application.Features.Students.Queries.GetStudents;

internal sealed class GetStudentsQueryHandler : IPaginateQueryHandler<GetStudentsQuery, SearchStudentDto>
{
    private readonly IGenericRepository<Student> _studentRepository;

    public GetStudentsQueryHandler(IGenericRepository<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Pagination<SearchStudentDto>> Handle(GetStudentsQuery query, CancellationToken cancellationToken)
    {
        var specification = new StudentFilterSpecification(query);

        var (studentsQuery, totalCount) = await _studentRepository.GetWithSpecAsync(specification);

        var students = await studentsQuery.ToListAsync(cancellationToken);

        var studentDtos = students.Select(student => new SearchStudentDto(
            student.Id,
            student.FullName,
            student.Email,
            student.Grade,
            student.GradeDisplay,
            student.Age,
            student.IsMinor))
            .ToList();

        return Pagination<SearchStudentDto>.Success(query.Page, query.PageSize, totalCount, studentDtos);
    }
}