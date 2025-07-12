namespace StudentTracker.Application.Features.Analytics.DTOs;

public class ClassSummaryDto
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public decimal AverageProgress { get; set; }
    public decimal AverageScore { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<GradeStatisticsDto> GradeStatistics { get; set; } = [];
    public List<PerformanceDistributionDto> PerformanceDistribution { get; set; } = [];
    public List<ActivityTrendDto> RecentActivityTrends { get; set; } = [];
}

public class GradeStatisticsDto
{
    public int Grade { get; set; }
    public int StudentCount { get; set; }
    public decimal AverageProgress { get; set; }
    public decimal AverageScore { get; set; }
    public int CompletedAssignments { get; set; }
    public int TotalAssignments { get; set; }
}



public class PerformanceDistributionDto
{
    public string PerformanceRange { get; set; } = string.Empty; 
    public int StudentCount { get; set; }
    public decimal Percentage { get; set; }
}

public class ActivityTrendDto
{
    public DateTime Date { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalActivities { get; set; }
    public int CompletedAssignments { get; set; }
    public int TotalTimeSpentMinutes { get; set; }
}