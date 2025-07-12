using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Students.DTOs;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Specification;

namespace StudentTracker.Application.Features.Students.Queries.GetStudentById;

internal sealed class GetStudentByIdQueryHandler : IQueryHandler<GetStudentByIdQuery, StudentDetailsDto>
{
    private readonly IGenericRepository<Student> _studentRepository;

    public GetStudentByIdQueryHandler(IGenericRepository<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<StudentDetailsDto>> Handle(GetStudentByIdQuery query, CancellationToken cancellationToken)
    {
        var specification = new StudentByIdSpecification(query.Id, includeDetails: true);
        var student = await _studentRepository.GetEntityWithSpecAsync(specification, cancellationToken);

        if (student is null)
        {
            return Result.Failure<StudentDetailsDto>(Error.NotFound("Student not found"));
        }

        var recentProgress = student.ProgressRecords
            .OrderByDescending(p => p.LastAccessedAt)
            .Take(5)
            .Select(p => new StudentProgressDto(
                p.Id,
                p.Assignment.Title,
                p.CompletionPercentage,
                p.TimeSpentMinutes,
                p.Status.ToString(),
                p.StartedAt,
                p.CompletedAt))
            .ToList();

        var recentAssessments = student.GetRecentAssessments()
            .Select(a => new StudentAssessmentDto(
                a.Id,
                a.Type.ToString(),
                a.PercentageScore,
                a.TakenAt))
            .ToList();

        var activeCourses = student.StudentCourses
            .Where(sc => sc.IsActive)
            .Select(sc => new StudentCourseDto(
                sc.CourseId,
                sc.Course.Name,
                sc.Course.Code,
                sc.EnrollmentDate,
                sc.CompletionDate,
                sc.IsActive))
            .ToList();

        var studentDto = new StudentDetailsDto(
            student.Id,
            student.FullName,
            student.Email,
            student.Grade,
            student.GradeDisplay,
            student.Age,
            student.IsMinor,
            student.ParentEmail!,
            student.DateOfBirth,
            student.GetOverallCompletionPercentage(),
            student.GetCompletedAssignmentsCount(),
            student.GetTotalAssignmentsCount(),
            student.GetAverageAssessmentScore(),
            recentProgress,
            recentAssessments,
            activeCourses);

        return Result.Success(studentDto);
    }
}