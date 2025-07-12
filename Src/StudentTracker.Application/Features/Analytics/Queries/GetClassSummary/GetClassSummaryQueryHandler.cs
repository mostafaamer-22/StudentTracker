using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Analytics.DTOs;
using StudentTracker.Application.Specifications;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using Application.Abstractions.Messaging;

namespace StudentTracker.Application.Features.Analytics.Queries.GetClassSummary;

internal sealed class GetClassSummaryQueryHandler : IQueryHandler<GetClassSummaryQuery, ClassSummaryDto>
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<StudentProgress> _progressRepository;
    private readonly IGenericRepository<Assessment> _assessmentRepository;
    private readonly IGenericRepository<Assignment> _assignmentRepository;
    private readonly IGenericRepository<Course> _courseRepository;

    public GetClassSummaryQueryHandler(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<StudentProgress> progressRepository,
        IGenericRepository<Assessment> assessmentRepository,
        IGenericRepository<Assignment> assignmentRepository,
        IGenericRepository<Course> courseRepository)
    {
        _studentRepository = studentRepository;
        _progressRepository = progressRepository;
        _assessmentRepository = assessmentRepository;
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<ClassSummaryDto>> Handle(GetClassSummaryQuery request, CancellationToken cancellationToken)
    {
        var studentsSpec = new StudentsForAnalyticsSpecification(
            request.Grade, request.Subject, request.TeacherId, request.StartDate, request.EndDate);
        var (studentQuery, _) = await _studentRepository.GetWithSpecAsync(studentsSpec, cancellationToken);
        var students = await studentQuery.ToListAsync(cancellationToken);

        if (!students.Any())
        {
            return Result.Success(new ClassSummaryDto());
        }

        var studentIds = students.Select(s => s.Id).ToList();

        var progressSpec = new StudentProgressForAnalyticsSpecification(studentIds, request.StartDate, request.EndDate);
        var (progressQuery, _) = await _progressRepository.GetWithSpecAsync(progressSpec, cancellationToken);
        var progressRecords = await progressQuery.ToListAsync(cancellationToken);

        var assessmentSpec = new AssessmentsForAnalyticsSpecification(studentIds, request.StartDate, request.EndDate);
        var (assessmentQuery, _) = await _assessmentRepository.GetWithSpecAsync(assessmentSpec, cancellationToken);
        var assessments = await assessmentQuery.ToListAsync(cancellationToken);

        var courseIds = students.SelectMany(s => s.StudentCourses.Where(sc => sc.IsActive).Select(sc => sc.CourseId)).Distinct().ToList();
        var assignmentSpec = new AssignmentsForAnalyticsSpecification(courseIds, request.StartDate, request.EndDate);
        var (assignmentQuery, _) = await _assignmentRepository.GetWithSpecAsync(assignmentSpec, cancellationToken);
        var assignments = await assignmentQuery.ToListAsync(cancellationToken);

        var totalStudents = students.Count;
        var activeStudents = students.Count(s => progressRecords.Any(p => p.StudentId == s.Id &&
            p.LastAccessedAt >= DateTime.UtcNow.AddDays(-7)));

        var averageProgress = progressRecords.Any() ?
            progressRecords.Average(p => p.CompletionPercentage) : 0;

        var averageScore = assessments.Any() ?
            assessments.Average(a => a.PercentageScore) : 0;

        var totalAssignments = assignments.Count;
        var completedAssignments = progressRecords.Count(p => p.Status == ProgressStatus.Completed);
        var overdueAssignments = progressRecords.Count(p => p.IsOverdue);

        var gradeStats = students.GroupBy(s => s.Grade)
            .Select(g => new GradeStatisticsDto
            {
                Grade = g.Key,
                StudentCount = g.Count(),
                AverageProgress = progressRecords.Where(p => g.Any(s => s.Id == p.StudentId)).Any() ?
                    progressRecords.Where(p => g.Any(s => s.Id == p.StudentId)).Average(p => p.CompletionPercentage) : 0,
                AverageScore = assessments.Where(a => g.Any(s => s.Id == a.StudentId)).Any() ?
                    assessments.Where(a => g.Any(s => s.Id == a.StudentId)).Average(a => a.PercentageScore) : 0,
                CompletedAssignments = progressRecords.Count(p => g.Any(s => s.Id == p.StudentId) && p.Status == ProgressStatus.Completed),
                TotalAssignments = progressRecords.Count(p => g.Any(s => s.Id == p.StudentId))
            }).ToList();

        var performanceDistribution = CalculatePerformanceDistribution(students, progressRecords);

        var recentActivityTrends = CalculateRecentActivityTrends(progressRecords, assessments);

        var summary = new ClassSummaryDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            AverageProgress = Math.Round(averageProgress, 2),
            AverageScore = Math.Round(averageScore, 2),
            TotalAssignments = totalAssignments,
            CompletedAssignments = completedAssignments,
            OverdueAssignments = overdueAssignments,
            LastUpdated = DateTime.UtcNow,
            GradeStatistics = gradeStats,
            PerformanceDistribution = performanceDistribution,
            RecentActivityTrends = recentActivityTrends
        };

        return Result.Success(summary);
    }

    private static List<PerformanceDistributionDto> CalculatePerformanceDistribution(
        List<Student> students, List<StudentProgress> progressRecords)
    {
        var studentAverages = students.Select(s => new
        {
            StudentId = s.Id,
            AverageProgress = progressRecords.Where(p => p.StudentId == s.Id).Any() ?
                progressRecords.Where(p => p.StudentId == s.Id).Average(p => p.CompletionPercentage) : 0
        }).ToList();

        var total = studentAverages.Count;
        if (total == 0) return [];

        var ranges = new[]
        {
            new { Range = "90-100%", Min = 90m, Max = 100m },
            new { Range = "80-89%", Min = 80m, Max = 89.99m },
            new { Range = "70-79%", Min = 70m, Max = 79.99m },
            new { Range = "60-69%", Min = 60m, Max = 69.99m },
            new { Range = "Below 60%", Min = 0m, Max = 59.99m }
        };

        return ranges.Select(range => new PerformanceDistributionDto
        {
            PerformanceRange = range.Range,
            StudentCount = studentAverages.Count(sa => sa.AverageProgress >= range.Min && sa.AverageProgress <= range.Max),
            Percentage = Math.Round((decimal)studentAverages.Count(sa => sa.AverageProgress >= range.Min && sa.AverageProgress <= range.Max) / total * 100, 2)
        }).ToList();
    }

    private static List<ActivityTrendDto> CalculateRecentActivityTrends(
        List<StudentProgress> progressRecords, List<Assessment> assessments)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-30);

        var trends = new List<ActivityTrendDto>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dateProgress = progressRecords.Where(p => p.CreatedOnUtc.Date == date).ToList();
            var dateAssessments = assessments.Where(a => a.TakenAt.Date == date).ToList();

            trends.Add(new ActivityTrendDto
            {
                Date = date,
                ActiveStudents = dateProgress.Select(p => p.StudentId).Distinct().Count(),
                TotalActivities = dateProgress.Count + dateAssessments.Count,
                CompletedAssignments = dateProgress.Count(p => p.Status == ProgressStatus.Completed),
                TotalTimeSpentMinutes = dateProgress.Sum(p => p.TimeSpentMinutes)
            });
        }

        return trends;
    }
}