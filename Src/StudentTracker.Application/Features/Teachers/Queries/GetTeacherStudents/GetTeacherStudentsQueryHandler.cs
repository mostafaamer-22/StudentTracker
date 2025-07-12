using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Features.Students.DTOs;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Specification;

namespace StudentTracker.Application.Features.Teachers.Queries.GetTeacherStudents;

internal sealed class GetTeacherStudentsQueryHandler : IPaginateQueryHandler<GetTeacherStudentsQuery, GetStudentsForSpecificalTeacherDto>
{
    private readonly IGenericRepository<Teacher> _teacherRepository;
    private readonly IGenericRepository<Student> _studentRepository;

    public GetTeacherStudentsQueryHandler(
        IGenericRepository<Teacher> teacherRepository,
        IGenericRepository<Student> studentRepository)
    {
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Pagination<GetStudentsForSpecificalTeacherDto>> Handle(GetTeacherStudentsQuery query, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdAsync(query.TeacherId, cancellationToken);
        if (teacher == null || !teacher.IsActive)
        {
            return Pagination<GetStudentsForSpecificalTeacherDto>
                .Success(query.Page, query.PageSize, 0, new List<GetStudentsForSpecificalTeacherDto>());
        }

        var specification = new StudentsByTeacherSpecification(
            query.TeacherId,
            query.Page,
            query.PageSize,
            query.SearchTerm);

        var (studentsQuery, totalCount) = await _studentRepository.GetWithSpecAsync(specification, cancellationToken);
        var students = await studentsQuery.ToListAsync(cancellationToken);

        var studentDtos = students.Select(student => new GetStudentsForSpecificalTeacherDto(
            student.Id,
            student.FullName,
            student.Email,
            student.Grade,
            student.Age))
            .ToList();

        return Pagination<GetStudentsForSpecificalTeacherDto>.Success(query.Page, query.PageSize, totalCount, studentDtos);
    }
}