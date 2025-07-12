using Bogus;
using Microsoft.Extensions.Logging;
using StudentTracker.Application.Abstractions.DataSeeder;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using StudentTracker.Domain.Repositories;

namespace StudentTracker.Infrastructure.Services.Seeders;

internal sealed class AssignmentSeeder : ISeeder
{
    private readonly IGenericRepository<Course> _courseRepository;
    private readonly IGenericRepository<Assignment> _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignmentSeeder> _logger;

    public int ExecutionOrder { get; set; } = 1; // Run after courses but before students

    public AssignmentSeeder(
        IGenericRepository<Course> courseRepository,
        IGenericRepository<Assignment> assignmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<AssignmentSeeder> logger)
    {
        _courseRepository = courseRepository;
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var assignmentsExist = _assignmentRepository.GetAll();
        if (assignmentsExist.Any())
        {
            _logger.LogInformation("Assignments already seeded. Skipping...");
            return;
        }

        var courses = _courseRepository.GetAll().ToList();
        if (!courses.Any())
        {
            _logger.LogWarning("No courses found. Please run CourseSeeder first.");
            return;
        }

        _logger.LogInformation("Starting assignment seeding...");

        var faker = new Faker();
        var batchSize = 100;
        var totalRecords = 0;

        foreach (var course in courses)
        {
            var assignmentCount = faker.Random.Int(5, 7);
            var assignments = new List<Assignment>();

            for (int i = 0; i < assignmentCount; i++)
            {
                var type = faker.Random.Enum<AssignmentType>();
                var title = type switch
                {
                    AssignmentType.Quiz => $"Quiz {i + 1}: {string.Join(" ", faker.Lorem.Words(3))}",
                    AssignmentType.Project => $"Project {i + 1}: {string.Join(" ", faker.Lorem.Words(3))}",
                    AssignmentType.Homework => $"Homework {i + 1}: {string.Join(" ", faker.Lorem.Words(3))}",
                    AssignmentType.Lab => $"Lab {i + 1}: {string.Join(" ", faker.Lorem.Words(3))}",
                    AssignmentType.Presentation => $"Presentation {i + 1}: {string.Join(" ", faker.Lorem.Words(3))}",
                    _ => $"Assignment {i + 1}: {string.Join(" ", faker.Lorem.Words(3))}"
                };

                var maxPoints = type switch
                {
                    AssignmentType.Quiz => faker.Random.Decimal(20m, 50m),
                    AssignmentType.Project => faker.Random.Decimal(80m, 150m),
                    AssignmentType.Homework => faker.Random.Decimal(30m, 70m),
                    AssignmentType.Lab => faker.Random.Decimal(50m, 100m),
                    AssignmentType.Presentation => faker.Random.Decimal(60m, 120m),
                    _ => faker.Random.Decimal(40m, 80m)
                };

                var estimatedMinutes = type switch
                {
                    AssignmentType.Quiz => faker.Random.Int(15, 45),
                    AssignmentType.Project => faker.Random.Int(120, 360),
                    AssignmentType.Homework => faker.Random.Int(45, 90),
                    AssignmentType.Lab => faker.Random.Int(90, 180),
                    AssignmentType.Presentation => faker.Random.Int(60, 120),
                    _ => faker.Random.Int(30, 60)
                };

                // Spread assignments throughout the course duration
                var daysIntoCourse = (int)(course.Duration.TotalDays * ((i + 1.0) / assignmentCount));
                var dueDate = course.StartDate.AddDays(daysIntoCourse);
                var openDate = dueDate.AddDays(-faker.Random.Int(7, 14)); // Open 1-2 weeks before due

                var assignment = Assignment.Create(
                    title,
                    faker.Lorem.Paragraph(),
                    type,
                    maxPoints,
                    dueDate,
                    estimatedMinutes,
                    course.Id,
                    openDate,
                    faker.Lorem.Paragraphs(2));

                assignments.Add(assignment);

                if (assignments.Count >= batchSize)
                {
                    await _assignmentRepository.AddRangeAsync(assignments);
                    await _unitOfWork.CompleteAsync();

                    totalRecords += assignments.Count;
                    _logger.LogInformation("Seeded {Count} assignments", assignments.Count);

                    assignments.Clear();
                }
            }

            if (assignments.Any())
            {
                await _assignmentRepository.AddRangeAsync(assignments);
                await _unitOfWork.CompleteAsync();

                totalRecords += assignments.Count;
                _logger.LogInformation("Seeded {Count} assignments", assignments.Count);
            }
        }

        _logger.LogInformation("Completed seeding {TotalRecords} assignments", totalRecords);
    }
}