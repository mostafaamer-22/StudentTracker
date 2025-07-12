using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Abstractions.DataSeeder;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using StudentTracker.Domain.Repositories;

namespace StudentTracker.Infrastructure.Services.Seeders;

internal sealed class StudentProgressSeeder : ISeeder
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<Assignment> _assignmentRepository;
    private readonly IGenericRepository<StudentProgress> _progressRepository;
    private readonly IGenericRepository<Assessment> _assessmentRepository;
    private readonly IGenericRepository<StudentCourse> _studentCourseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentProgressSeeder> _logger;

    public int ExecutionOrder { get; set; } = 3; // Run after students are created and enrolled

    public StudentProgressSeeder(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<Assignment> assignmentRepository,
        IGenericRepository<StudentProgress> progressRepository,
        IGenericRepository<Assessment> assessmentRepository,
        IGenericRepository<StudentCourse> studentCourseRepository,
        IUnitOfWork unitOfWork,
        ILogger<StudentProgressSeeder> logger)
    {
        _studentRepository = studentRepository;
        _assignmentRepository = assignmentRepository;
        _progressRepository = progressRepository;
        _assessmentRepository = assessmentRepository;
        _studentCourseRepository = studentCourseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var progressExists = _progressRepository.GetAll();
        if (progressExists.Any())
        {
            _logger.LogInformation("Student progress already seeded. Skipping...");
            return;
        }

        var students = _studentRepository.GetAll().ToList();
        var assignments = _assignmentRepository.GetAll();
        var enrollments = _studentCourseRepository.GetAll();

        if (!students.Any() || !assignments.Any() || !enrollments.Any())
        {
            _logger.LogWarning("No students, assignments, or enrollments found. Skipping progress seeding.");
            return;
        }

        _logger.LogInformation("Starting student progress seeding...");

        var faker = new Faker();
        var batchSize = 100;
        var totalRecords = 0;

        var selectedStudents = faker.PickRandom(students, 50).ToList();
        _logger.LogInformation("Selected {Count} students for progress seeding", selectedStudents.Count);

        foreach (var student in selectedStudents)
        {
            var enrolledCourseIds = enrollments
                .Where(e => e.StudentId == student.Id)
                .Select(e => e.CourseId)
                .ToList();

            var studentAssignments = assignments
                .Where(a => enrolledCourseIds.Contains(a.CourseId))
                .ToList();

            if (!studentAssignments.Any()) continue;

            var progressRecords = new List<StudentProgress>();
            var assessments = new List<Assessment>();

            var assignmentCount = faker.Random.Int((int)(studentAssignments.Count * 0.7), (int)(studentAssignments.Count * 0.9));
            var selectedAssignments = faker.Random.ListItems(studentAssignments, assignmentCount).ToList();

            foreach (var assignment in selectedAssignments)
            {
                var startDate = faker.Date.Between(DateTime.Today.AddMonths(-6), DateTime.Today);
                var weights = new[] { 0.1f, 0.3f, 0.6f };
                var statuses = new[] { ProgressStatus.NotStarted, ProgressStatus.InProgress, ProgressStatus.Completed };
                var status = faker.Random.WeightedRandom(statuses, weights);

                var completionPercentage = status switch
                {
                    ProgressStatus.NotStarted => 0m,
                    ProgressStatus.InProgress => faker.Random.Decimal(10m, 90m),
                    ProgressStatus.Completed => 100m,
                    _ => 0m
                };

                var timeSpentMinutes = faker.Random.Int(15, 120);
                var earnedPoints = status == ProgressStatus.Completed ?
                    (decimal?)faker.Random.Decimal(assignment.MaxPoints * 0.6m, assignment.MaxPoints) :
                    null;

                var progress = StudentProgress.Create(
                    completionPercentage,
                    timeSpentMinutes,
                    status,
                    earnedPoints,
                    startDate,
                    status == ProgressStatus.Completed ? startDate.AddDays(faker.Random.Int(1, 14)) : null,
                    startDate.AddHours(faker.Random.Int(1, 24)),
                    faker.Random.Int(1, 5),
                    status == ProgressStatus.InProgress ? faker.Lorem.Sentence() : null,
                    student.Id,
                    assignment.Id);

                progressRecords.Add(progress);

                // Generate assessment for completed assignments (70% chance)
                if (status == ProgressStatus.Completed && faker.Random.Bool(0.7f))
                {
                    var score = faker.Random.Decimal(assignment.MaxPoints * 0.6m, assignment.MaxPoints);
                    var assessment = Assessment.Create(
                        score,
                        assignment.MaxPoints,
                        AssessmentType.Quiz,
                        progress.CompletedAt!.Value.AddDays(1),
                        faker.Random.Int(15, 60),
                        1,
                        student.Id,
                        assignment.Id,
                        faker.Random.Bool(0.3f) ? faker.Lorem.Sentence() : null);

                    assessments.Add(assessment);
                }

                if (progressRecords.Count >= batchSize)
                {
                    await _progressRepository.AddRangeAsync(progressRecords);
                    await _assessmentRepository.AddRangeAsync(assessments);
                    await _unitOfWork.CompleteAsync();

                    totalRecords += progressRecords.Count;
                    _logger.LogInformation("Seeded {Count} progress records", progressRecords.Count);

                    progressRecords.Clear();
                    assessments.Clear();
                }
            }

            if (progressRecords.Any())
            {
                await _progressRepository.AddRangeAsync(progressRecords);
                await _assessmentRepository.AddRangeAsync(assessments);
                await _unitOfWork.CompleteAsync();

                totalRecords += progressRecords.Count;
                _logger.LogInformation("Seeded {Count} progress records", progressRecords.Count);
            }
        }

        _logger.LogInformation("Completed seeding {TotalRecords} student progress records", totalRecords);
    }
}