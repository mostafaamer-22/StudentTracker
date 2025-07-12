using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StudentTracker.Application.Abstractions.DataSeeder;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using System.Collections.Concurrent;

namespace StudentTracker.Infrastructure.Services.Seeders;

internal sealed class CourseSeeder : ISeeder
{
    private readonly IGenericRepository<Course> _courseRepository;
    private readonly IGenericRepository<Teacher> _teacherRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CourseSeeder> _logger;
    private readonly UserManager<User> _userManager;
    private readonly ConcurrentDictionary<string, byte> _usedUsernames = new();
    private readonly ConcurrentDictionary<string, byte> _usedEmails = new();

    public int ExecutionOrder { get; set; } = 0; // Run before student seeder

    public CourseSeeder(
        IGenericRepository<Course> courseRepository,
        IGenericRepository<Teacher> teacherRepository,
        IUnitOfWork unitOfWork,
        ILogger<CourseSeeder> logger,
        UserManager<User> userManager)
    {
        _courseRepository = courseRepository;
        _teacherRepository = teacherRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userManager = userManager;
    }

    private const string DefaultPassword = "Password123!";

    private string GenerateUniqueUsername(Faker f, string firstName, string lastName)
    {
        string username;
        int attempt = 0;
        do
        {
            username = attempt == 0
                ? f.Internet.UserName(firstName, lastName).ToUpper()
                : f.Internet.UserName(firstName, lastName).ToUpper() + attempt.ToString();
            attempt++;
        } while (!_usedUsernames.TryAdd(username, 1));

        return username;
    }

    private string GenerateUniqueEmail(Faker f, string firstName, string lastName)
    {
        string email;
        int attempt = 0;
        do
        {
            email = attempt == 0
                ? f.Internet.Email(firstName, lastName, "Teacher.edu").ToLower()
                : f.Internet.Email(firstName + attempt, lastName, "Teacher.edu").ToLower();
            attempt++;
        } while (!_usedEmails.TryAdd(email, 1));

        return email;
    }

    public async Task SeedAsync()
    {
        var coursesExist = _courseRepository.GetAll();
        if (coursesExist.Any())
        {
            _logger.LogInformation("Courses already seeded. Skipping...");
            return;
        }

        _logger.LogInformation("Starting course and teacher seeding...");

        var teacherFaker = new Faker<Teacher>()
            .CustomInstantiator(f =>
            {
                var firstName = f.Name.FirstName();
                var lastName = f.Name.LastName();
                var email = GenerateUniqueEmail(f, firstName, lastName);
                var department = f.PickRandom(new[] { "Math", "Science", "English", "History", "Computer Science" });
                var qualifications = f.Random.ArrayElement(new[] {
                    "Ph.D. in Education",
                    "Master's in Teaching",
                    "B.Ed with 10 years experience",
                    "M.Ed with specialization"
                });

                var teacher = Teacher.Create(
                    firstName,
                    lastName,
                    email,
                    department,
                    qualifications);

                var username = GenerateUniqueUsername(f, firstName, lastName);
                teacher.UserName = username;
                teacher.NormalizedUserName = username.ToUpperInvariant();
                teacher.NormalizedEmail = email.ToUpperInvariant();

                return teacher;
            });

        var teachers = teacherFaker.Generate(20);
        var createdTeachers = new List<Teacher>();

        foreach (var teacher in teachers)
        {
            var result = await _userManager.CreateAsync(teacher, DefaultPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(teacher, "Teacher");
                createdTeachers.Add(teacher);
                _logger.LogInformation("Created teacher account and assigned 'Teacher' role to {Email}.", teacher.Email);
            }
            else
            {
                _logger.LogError("Failed to create teacher {Email}: {Errors}",
                    teacher.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation("Created {Count} teacher accounts", createdTeachers.Count);

        var startDate = DateTime.Today;
        var coursesPerGrade = 3;
        var courses = new List<Course>();

        for (int grade = 1; grade <= 12; grade++)
        {
            var gradeCourses = Enumerable.Range(0, coursesPerGrade).Select(_ =>
            {
                var f = new Faker();
                var teacher = f.PickRandom(teachers);
                var subject = f.PickRandom(new[] {
                    "Algebra", "Geometry", "Biology", "Chemistry", "Physics",
                    "World History", "Literature", "Programming", "Art", "Music"
                });
                var courseCode = $"{subject.Substring(0, 3).ToUpper()}{grade}{f.Random.Number(100, 999)}";

                return Course.Create(
                    $"{subject} {grade}",
                    courseCode,
                    f.Lorem.Paragraph(),
                    grade,
                    teacher.Id,
                    startDate.AddDays(f.Random.Int(-5, 5)),
                    startDate.AddMonths(f.Random.Int(3, 6)),
                    f.Random.Decimal(80, 120));
            }).ToList();

            courses.AddRange(gradeCourses);
        }

        var remainingCourses = Math.Max(0, 100 - courses.Count);
        if (remainingCourses > 0)
        {
            var courseFaker = new Faker<Course>()
                .CustomInstantiator(f =>
                {
                    var teacher = f.PickRandom(teachers);
                    var subject = f.PickRandom(new[] {
                        "Algebra", "Geometry", "Biology", "Chemistry", "Physics",
                        "World History", "Literature", "Programming", "Art", "Music"
                    });
                    var gradeLevel = f.Random.Int(1, 12);
                    var courseCode = $"{subject.Substring(0, 3).ToUpper()}{gradeLevel}{f.Random.Number(100, 999)}";

                    return Course.Create(
                        $"{subject} {gradeLevel}",
                        courseCode,
                        f.Lorem.Paragraph(),
                        gradeLevel,
                        teacher.Id,
                        startDate.AddDays(f.Random.Int(-5, 5)),
                        startDate.AddMonths(f.Random.Int(3, 6)),
                        f.Random.Decimal(80, 120));
                });

            courses.AddRange(courseFaker.Generate(remainingCourses));
        }

        var batchSize = 50;
        var batches = (int)Math.Ceiling(courses.Count / (double)batchSize);

        for (int i = 0; i < batches; i++)
        {
            var batchCourses = courses.Skip(i * batchSize).Take(batchSize).ToList();
            await _courseRepository.AddRangeAsync(batchCourses);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Seeded batch {BatchNumber} of {TotalBatches} ({Count} courses)",
                i + 1, batches, batchCourses.Count);
        }

        _logger.LogInformation("Completed seeding courses and teachers");
    }
}