using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Features.Students.DTOs;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Specification;

namespace StudentTracker.Application.Features.Students.Queries.GetStudentProgress;

internal sealed class GetStudentProgressQueryHandler
    : IQueryHandler<GetStudentProgressQuery, StudentProgressMetricsDto>
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<StudentProgress> _progressRepository;
    private readonly IGenericRepository<Assessment> _assessmentRepository;
    private readonly IGenericRepository<Assignment> _assignmentRepository;

    public GetStudentProgressQueryHandler(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<StudentProgress> progressRepository,
        IGenericRepository<Assessment> assessmentRepository,
        IGenericRepository<Assignment> assignmentRepository)
    {
        _studentRepository = studentRepository;
        _progressRepository = progressRepository;
        _assessmentRepository = assessmentRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<Result<StudentProgressMetricsDto>> Handle(
        GetStudentProgressQuery query,
        CancellationToken cancellationToken)
    {
        var studentSpec = new StudentBasicSpecification(query.StudentId);
        var student = await _studentRepository.GetEntityWithSpecAsync(studentSpec, cancellationToken);

        if (student == null)
        {
            return Result.Failure<StudentProgressMetricsDto>(Error.NotFound("Student not found"));
        }

        var progressSpec = new StudentProgressRecordsSpecification(query.StudentId, query.StartDate, query.EndDate);
        var (progressQuery, _) = await _progressRepository.GetWithSpecAsync(progressSpec, cancellationToken);
        var progressRecords = await progressQuery.ToListAsync(cancellationToken);

        var assessmentSpec = new StudentAssessmentsSpecification(query.StudentId, query.StartDate, query.EndDate);
        var (assessmentQuery, _) = await _assessmentRepository.GetWithSpecAsync(assessmentSpec, cancellationToken);
        var assessments = await assessmentQuery.ToListAsync(cancellationToken);

        var assignmentSpec = new StudentCourseAssignmentsSpecification(query.StudentId);
        var (assignmentQuery, _) = await _assignmentRepository.GetWithSpecAsync(assignmentSpec, cancellationToken);
        var assignments = await assignmentQuery.ToListAsync(cancellationToken);

        var courseProgress = CalculateCourseProgress(student, progressRecords, assessments, assignments);

        var monthlyProgress = CalculateMonthlyProgress(progressRecords, assessments);

        var recentActivities = GetRecentActivities(progressRecords, assessments);

        var overallCompletion = progressRecords.Any()
            ? progressRecords.Average(p => p.CompletionPercentage)
            : 0;

        var completedAssignments = progressRecords.Count(p =>
            p.Status == Domain.Enums.ProgressStatus.Completed);

        var totalAssignments = assignments.Count;

        var averageAssessmentScore = assessments.Any()
            ? assessments.Average(a => a.PercentageScore)
            : 0;

        var metrics = new StudentProgressMetricsDto(
            overallCompletion,
            completedAssignments,
            totalAssignments,
            averageAssessmentScore,
            courseProgress,
            monthlyProgress,
            recentActivities);

        return Result.Success(metrics);
    }

    private List<CourseProgressDto> CalculateCourseProgress(
    Student student,
    List<StudentProgress> progressRecords,
    List<Assessment> assessments,
    List<Assignment> assignments)
    {
        return student.StudentCourses
            .Where(sc => sc.IsActive)
            .Select(sc =>
            {
                var courseAssignments = assignments.Where(a => a.CourseId == sc.CourseId).ToList();

                var completedCount = courseAssignments.Count(a =>
                    progressRecords.Any(p => p.AssignmentId == a.Id &&
                                            p.Status == Domain.Enums.ProgressStatus.Completed));

                var avgCompletion = courseAssignments
                    .Where(a => progressRecords.Any(p => p.AssignmentId == a.Id))
                    .Select(a => progressRecords
                        .Where(p => p.AssignmentId == a.Id)
                        .Average(p => p.CompletionPercentage))
                    .DefaultIfEmpty(0)
                    .Average();

                var avgAssessmentScore = courseAssignments
                    .Where(a => assessments.Any(asmt => asmt.AssignmentId == a.Id))
                    .Select(a => assessments
                        .Where(asmt => asmt.AssignmentId == a.Id)
                        .Average(asmt => asmt.PercentageScore))
                    .DefaultIfEmpty(0)
                    .Average();

                return new CourseProgressDto(
                    sc.Course.Name,
                    avgCompletion,
                    completedCount,
                    courseAssignments.Count,
                    avgAssessmentScore);
            })
            .ToList();
    }

    private List<MonthlyProgressDto> CalculateMonthlyProgress(
        List<StudentProgress> progressRecords,
        List<Assessment> assessments)
    {
        return progressRecords
            .Where(p => p.StartedAt.HasValue)
            .GroupBy(p => new DateTime(p.StartedAt!.Value.Year, p.StartedAt.Value.Month, 1))
            .Select(g =>
            {
                var monthAssessments = assessments
                    .Where(a => a.TakenAt.Year == g.Key.Year && a.TakenAt.Month == g.Key.Month);

                return new MonthlyProgressDto(
                    g.Key,
                    g.Average(p => p.CompletionPercentage),
                    g.Count(p => p.Status == Domain.Enums.ProgressStatus.Completed),
                    monthAssessments.Any() ? monthAssessments.Average(a => a.PercentageScore) : 0);
            })
            .OrderByDescending(m => m.Month)
            .ToList();
    }

    private List<RecentActivityDto> GetRecentActivities(
        List<StudentProgress> progressRecords,
        List<Assessment> assessments)
    {
        var progressActivities = progressRecords
            .OrderByDescending(p => p.LastAccessedAt)
            .Take(10)
            .Select(p => new RecentActivityDto(
                p.Id,
                p.Assignment.Title,
                Domain.Enums.ActivityType.AssignmentStarted,
                p.LastAccessedAt,
                null,
                p.CompletionPercentage,
                p.Status.ToString()));

        var assessmentActivities = assessments
            .OrderByDescending(a => a.TakenAt)
            .Take(10)
            .Select(a => new RecentActivityDto(
                a.Id,
                a.Assignment.Title,
                Domain.Enums.ActivityType.QuizTaken,
                a.TakenAt,
                a.PercentageScore,
                null,
                "Completed"));

        return progressActivities
            .Concat(assessmentActivities)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();
    }
}