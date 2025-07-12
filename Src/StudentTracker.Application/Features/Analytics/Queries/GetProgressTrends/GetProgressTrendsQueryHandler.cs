using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Analytics.DTOs;
using StudentTracker.Application.Specifications;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using Application.Abstractions.Messaging;

namespace StudentTracker.Application.Features.Analytics.Queries.GetProgressTrends;

internal sealed class GetProgressTrendsQueryHandler : IQueryHandler<GetProgressTrendsQuery, ProgressTrendsDto>
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<StudentProgress> _progressRepository;
    private readonly IGenericRepository<Assessment> _assessmentRepository;
    private readonly IGenericRepository<Course> _courseRepository;

    public GetProgressTrendsQueryHandler(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<StudentProgress> progressRepository,
        IGenericRepository<Assessment> assessmentRepository,
        IGenericRepository<Course> courseRepository)
    {
        _studentRepository = studentRepository;
        _progressRepository = progressRepository;
        _assessmentRepository = assessmentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<ProgressTrendsDto>> Handle(GetProgressTrendsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow.Date;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        if (startDate > endDate)
        {
            return Result.Failure<ProgressTrendsDto>(Error.Validation("StartDate cannot be after EndDate"));
        }

        var studentsSpec = new StudentsForAnalyticsSpecification(
            request.Grade, request.Subject, request.TeacherId, startDate, endDate);
        var (studentQuery, _) = await _studentRepository.GetWithSpecAsync(studentsSpec, cancellationToken);
        var students = await studentQuery.ToListAsync(cancellationToken);

        if (!students.Any())
        {
            return Result.Success(new ProgressTrendsDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Period = request.Period.ToLower()
            });
        }

        var studentIds = students.Select(s => s.Id).ToList();

        var progressSpec = new StudentProgressForAnalyticsSpecification(studentIds, startDate, endDate);
        var (progressQuery, _) = await _progressRepository.GetWithSpecAsync(progressSpec, cancellationToken);
        var progressRecords = await progressQuery.ToListAsync(cancellationToken);

        var assessmentSpec = new AssessmentsForAnalyticsSpecification(studentIds, startDate, endDate);
        var (assessmentQuery, _) = await _assessmentRepository.GetWithSpecAsync(assessmentSpec, cancellationToken);
        var assessments = await assessmentQuery.ToListAsync(cancellationToken);

        var courseIds = students.SelectMany(s => s.StudentCourses.Where(sc => sc.IsActive).Select(sc => sc.CourseId)).Distinct().ToList();
        var courseSpec = new CoursesForAnalyticsSpecification(request.Grade, request.Subject, request.TeacherId);
        var (courseQuery, _) = await _courseRepository.GetWithSpecAsync(courseSpec, cancellationToken);
        var courses = await courseQuery.ToListAsync(cancellationToken);

        var overallProgressTrend = CalculateProgressTrend(progressRecords, startDate, endDate, request.Period);
        var scoreTrend = CalculateScoreTrend(assessments, startDate, endDate, request.Period);
        var activityTrend = CalculateActivityTrend(progressRecords, assessments, startDate, endDate, request.Period);
        var subjectTrends = CalculateSubjectTrends(progressRecords, assessments, courses, startDate, endDate, request.Period);
        var gradeTrends = CalculateGradeTrends(progressRecords, assessments, students, startDate, endDate, request.Period);
        var comparison = CalculateComparison(progressRecords, startDate, endDate, request.Period);

        var result = new ProgressTrendsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Period = request.Period.ToLower(),
            OverallProgressTrend = overallProgressTrend,
            ScoreTrend = scoreTrend,
            ActivityTrend = activityTrend,
            SubjectTrends = subjectTrends,
            GradeTrends = gradeTrends,
            Comparison = comparison
        };

        return Result.Success(result);
    }

    private static List<TrendDataPointDto> CalculateProgressTrend(
        List<StudentProgress> progressRecords, DateTime startDate, DateTime endDate, string period)
    {
        var trends = new List<TrendDataPointDto>();
        var dateInterval = GetDateInterval(period);

        for (var date = startDate; date <= endDate; date = date.Add(dateInterval))
        {
            var periodEnd = date.Add(dateInterval).AddDays(-1);
            if (periodEnd > endDate) periodEnd = endDate;

            var periodRecords = progressRecords.Where(p =>
                p.CreatedOnUtc.Date >= date && p.CreatedOnUtc.Date <= periodEnd).ToList();

            trends.Add(new TrendDataPointDto
            {
                Date = date,
                Value = periodRecords.Any() ? Math.Round(periodRecords.Average(p => p.CompletionPercentage), 2) : 0,
                Count = periodRecords.Count,
                Label = GetPeriodLabel(date, period)
            });
        }

        return trends;
    }

    private static List<TrendDataPointDto> CalculateScoreTrend(
        List<Assessment> assessments, DateTime startDate, DateTime endDate, string period)
    {
        var trends = new List<TrendDataPointDto>();
        var dateInterval = GetDateInterval(period);

        for (var date = startDate; date <= endDate; date = date.Add(dateInterval))
        {
            var periodEnd = date.Add(dateInterval).AddDays(-1);
            if (periodEnd > endDate) periodEnd = endDate;

            var periodAssessments = assessments.Where(a =>
                a.TakenAt.Date >= date && a.TakenAt.Date <= periodEnd).ToList();

            trends.Add(new TrendDataPointDto
            {
                Date = date,
                Value = periodAssessments.Any() ? Math.Round(periodAssessments.Average(a => a.PercentageScore), 2) : 0,
                Count = periodAssessments.Count,
                Label = GetPeriodLabel(date, period)
            });
        }

        return trends;
    }

    private static List<TrendDataPointDto> CalculateActivityTrend(
        List<StudentProgress> progressRecords, List<Assessment> assessments,
        DateTime startDate, DateTime endDate, string period)
    {
        var trends = new List<TrendDataPointDto>();
        var dateInterval = GetDateInterval(period);

        for (var date = startDate; date <= endDate; date = date.Add(dateInterval))
        {
            var periodEnd = date.Add(dateInterval).AddDays(-1);
            if (periodEnd > endDate) periodEnd = endDate;

            var periodProgress = progressRecords.Where(p =>
                p.CreatedOnUtc.Date >= date && p.CreatedOnUtc.Date <= periodEnd).ToList();
            var periodAssessments = assessments.Where(a =>
                a.TakenAt.Date >= date && a.TakenAt.Date <= periodEnd).ToList();

            var totalActivities = periodProgress.Count + periodAssessments.Count;
            var activeStudents = periodProgress.Select(p => p.StudentId)
                .Concat(periodAssessments.Select(a => a.StudentId))
                .Distinct()
                .Count();

            trends.Add(new TrendDataPointDto
            {
                Date = date,
                Value = activeStudents,
                Count = totalActivities,
                Label = GetPeriodLabel(date, period)
            });
        }

        return trends;
    }

    private static List<SubjectTrendDto> CalculateSubjectTrends(
        List<StudentProgress> progressRecords, List<Assessment> assessments,
        List<Course> courses, DateTime startDate, DateTime endDate, string period)
    {
        return courses.GroupBy(c => new { c.Name, c.Code })
            .Select(g => new SubjectTrendDto
            {
                SubjectName = g.Key.Name,
                SubjectCode = g.Key.Code,
                ProgressTrend = CalculateProgressTrend(progressRecords, startDate, endDate, period),
                ScoreTrend = CalculateScoreTrend(assessments, startDate, endDate, period)
            }).ToList();
    }

    private static List<GradeTrendDto> CalculateGradeTrends(
        List<StudentProgress> progressRecords, List<Assessment> assessments,
        List<Student> students, DateTime startDate, DateTime endDate, string period)
    {
        return students.GroupBy(s => s.Grade)
            .Select(g => new GradeTrendDto
            {
                Grade = g.Key,
                ProgressTrend = CalculateProgressTrend(
                    progressRecords.Where(p => g.Any(s => s.Id == p.StudentId)).ToList(),
                    startDate, endDate, period),
                ScoreTrend = CalculateScoreTrend(
                    assessments.Where(a => g.Any(s => s.Id == a.StudentId)).ToList(),
                    startDate, endDate, period)
            }).ToList();
    }

    private static ProgressComparisonDto CalculateComparison(
        List<StudentProgress> progressRecords, DateTime startDate, DateTime endDate, string period)
    {
        var periodDays = (endDate - startDate).Days + 1;
        var currentPeriodRecords = progressRecords.Where(p => p.CreatedOnUtc.Date >= startDate && p.CreatedOnUtc.Date <= endDate).ToList();

        var previousStartDate = startDate.AddDays(-periodDays);
        var previousEndDate = startDate.AddDays(-1);
        var previousPeriodRecords = progressRecords.Where(p => p.CreatedOnUtc.Date >= previousStartDate && p.CreatedOnUtc.Date <= previousEndDate).ToList();

        var currentAverage = currentPeriodRecords.Any() ? currentPeriodRecords.Average(p => p.CompletionPercentage) : 0;
        var previousAverage = previousPeriodRecords.Any() ? previousPeriodRecords.Average(p => p.CompletionPercentage) : 0;

        var percentageChange = previousAverage > 0 ?
            Math.Round((currentAverage - previousAverage) / previousAverage * 100, 2) : 0;

        return new ProgressComparisonDto
        {
            CurrentPeriodAverage = Math.Round(currentAverage, 2),
            PreviousPeriodAverage = Math.Round(previousAverage, 2),
            PercentageChange = percentageChange,
            Trend = percentageChange > 0 ? "Improving" : percentageChange < 0 ? "Declining" : "Stable",
            DaysInPeriod = periodDays
        };
    }

    private static TimeSpan GetDateInterval(string period)
    {
        return period.ToLower() switch
        {
            "daily" => TimeSpan.FromDays(1),
            "weekly" => TimeSpan.FromDays(7),
            "monthly" => TimeSpan.FromDays(30),
            _ => TimeSpan.FromDays(7)
        };
    }

    private static string GetPeriodLabel(DateTime date, string period)
    {
        return period.ToLower() switch
        {
            "daily" => date.ToString("MM/dd"),
            "weekly" => $"Week of {date:MM/dd}",
            "monthly" => date.ToString("MMM yyyy"),
            _ => date.ToString("MM/dd")
        };
    }
}