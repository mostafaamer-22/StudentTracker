namespace StudentTracker.Application.Features.Analytics.DTOs;

public class ProgressTrendsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Period { get; set; } = string.Empty; 
    public List<TrendDataPointDto> OverallProgressTrend { get; set; } = [];
    public List<TrendDataPointDto> ScoreTrend { get; set; } = [];
    public List<TrendDataPointDto> ActivityTrend { get; set; } = [];
    public List<SubjectTrendDto> SubjectTrends { get; set; } = [];
    public List<GradeTrendDto> GradeTrends { get; set; } = [];
    public ProgressComparisonDto Comparison { get; set; } = null!;
}

public class TrendDataPointDto
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public int Count { get; set; }
    public string? Label { get; set; }
}

public class SubjectTrendDto
{
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public List<TrendDataPointDto> ProgressTrend { get; set; } = [];
    public List<TrendDataPointDto> ScoreTrend { get; set; } = [];
}

public class GradeTrendDto
{
    public int Grade { get; set; }
    public List<TrendDataPointDto> ProgressTrend { get; set; } = [];
    public List<TrendDataPointDto> ScoreTrend { get; set; } = [];
}

public class ProgressComparisonDto
{
    public decimal CurrentPeriodAverage { get; set; }
    public decimal PreviousPeriodAverage { get; set; }
    public decimal PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty; 
    public int DaysInPeriod { get; set; }
}