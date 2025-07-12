using Microsoft.Extensions.Configuration;

namespace StudentTracker.Infrastructure.Options;

public class SeederSettings
{
    public const string SectionName = "SeederSettings";

    public bool EnableSeeding { get; set; } = true;
    public int BatchSize { get; set; } = 50;
    public int StudentCount { get; set; } = 100;
    public int CourseCount { get; set; } = 20;
    public int TeacherCount { get; set; } = 10;
    public int AssignmentsPerCourse { get; set; } = 5;
    public int StudentsForProgress { get; set; } = 20;
    public bool GenerateMinimalData { get; set; } = true;
}