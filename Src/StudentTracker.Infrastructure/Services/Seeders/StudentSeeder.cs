using Bogus;
using Microsoft.Extensions.Logging;
using StudentTracker.Application.Abstractions.DataSeeder;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using StudentTracker.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace StudentTracker.Infrastructure.Services.Seeders;

internal sealed class StudentSeeder : ISeeder
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<Course> _courseRepository;
    private readonly IGenericRepository<StudentCourse> _studentCourseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentSeeder> _logger;
    private readonly UserManager<User> _userManager;
    private readonly SeederSettings _settings;
    private readonly ConcurrentDictionary<string, byte> _usedUsernames = new();
    private readonly ConcurrentDictionary<string, byte> _usedEmails = new();

    private const string DefaultPassword = "Password123!";
    private const int MaxParallelTasks = 10;

    public int ExecutionOrder { get; set; } = 2;

    public StudentSeeder(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<Course> courseRepository,
        IGenericRepository<StudentCourse> studentCourseRepository,
        IUnitOfWork unitOfWork,
        ILogger<StudentSeeder> logger,
        UserManager<User> userManager,
        IOptions<SeederSettings> settings)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _studentCourseRepository = studentCourseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userManager = userManager;
        _settings = settings.Value;
    }

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
                ? f.Internet.Email(firstName, lastName, "school.edu").ToLower()
                : f.Internet.Email(firstName + attempt, lastName, "school.edu").ToLower();
            attempt++;
        } while (!_usedEmails.TryAdd(email, 1));

        return email;
    }

    private async Task<List<StudentCourse>> GenerateEnrollmentsForStudent(
        Student student,
        IList<Course> eligibleCourses,
        Faker enrollmentFaker)
    {
        if (!eligibleCourses.Any()) return new List<StudentCourse>();

        var maxCoursesToEnroll = _settings.GenerateMinimalData ?
            Math.Min(2, eligibleCourses.Count) :
            Math.Min(4, eligibleCourses.Count);
        var minCoursesToEnroll = Math.Min(1, maxCoursesToEnroll);
        var coursesToEnroll = enrollmentFaker.Random.Int(minCoursesToEnroll, maxCoursesToEnroll);
        var selectedCourses = enrollmentFaker.PickRandom(eligibleCourses, coursesToEnroll);

        return selectedCourses.Select(course =>
            StudentCourse.Create(student.Id, course.Id)).ToList();
    }

    public async Task SeedAsync()
    {
        if (!_settings.EnableSeeding)
        {
            _logger.LogInformation("Seeding is disabled. Skipping student seeding...");
            return;
        }

        var studentsExists = _studentRepository.GetAll();
        if (studentsExists.Any())
        {
            _logger.LogInformation("Students already seeded. Skipping...");
            return;
        }

        var courses = _courseRepository.GetAll().ToList();
        if (!courses.Any())
        {
            _logger.LogWarning("No courses found. Please run CourseSeeder first.");
            return;
        }

        _logger.LogInformation("Starting student seeding with default password: {DefaultPassword}", DefaultPassword);

        var faker = new Faker<Student>()
            .CustomInstantiator(f =>
            {
                var firstName = f.Name.FirstName();
                var lastName = f.Name.LastName();
                var email = GenerateUniqueEmail(f, firstName, lastName);
                var grade = f.Random.Int(1, 12);
                var dateOfBirth = f.Date.Past(18, DateTime.Today.AddYears(-5))
                    .Date
                    .AddYears(-grade);
                var parentEmail = f.Internet.Email(firstName + "parent", lastName, "gmail.com");

                var student = Student.Create(
                    firstName,
                    lastName,
                    email,
                    grade,
                    dateOfBirth,
                    parentEmail);

                var username = GenerateUniqueUsername(f, firstName, lastName);
                student.UserName = username;
                student.NormalizedUserName = username.ToUpperInvariant();
                student.NormalizedEmail = email.ToUpperInvariant();

                return student;
            });

        var batchSize = _settings.BatchSize;
        var totalStudents = _settings.StudentCount;
        var batches = (int)Math.Ceiling(totalStudents / (double)batchSize);
        var enrollmentFaker = new Faker();
        var coursesByGrade = courses.GroupBy(c => c.GradeLevel)
            .ToDictionary(g => g.Key, g => g.Where(c => c.IsActive).ToList());

        for (int i = 0; i < batches; i++)
        {
            var currentBatchSize = Math.Min(batchSize, totalStudents - (i * batchSize));
            var students = faker.Generate(currentBatchSize);
            var createdStudents = new List<Student>();
            var enrollments = new List<StudentCourse>();

            foreach (var student in students)
            {
                var result = await _userManager.CreateAsync(student, DefaultPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(student, "Student");
                    _logger.LogInformation("Assigned 'Student' role to {Email}.", student.Email);
                }
                else
                {
                    _logger.LogError("Failed to create student {Email}: {Errors}",
                        student.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    continue;
                }

                createdStudents.Add(student);

                if (coursesByGrade.TryGetValue(student.Grade, out var eligibleCourses))
                {
                    var studentEnrollments = await GenerateEnrollmentsForStudent(
                        student, eligibleCourses, enrollmentFaker);
                    enrollments.AddRange(studentEnrollments);
                }
            }

            if (enrollments.Any())
            {
                await _studentCourseRepository.AddRangeAsync(enrollments);
                await _unitOfWork.CompleteAsync();
            }

            _logger.LogInformation("Seeded batch {BatchNumber} of {TotalBatches} ({Count} students with {EnrollmentCount} enrollments)",
                i + 1, batches, createdStudents.Count, enrollments.Count);
        }

        _logger.LogInformation("Completed seeding {TotalStudents} students with default password", totalStudents);
    }
}