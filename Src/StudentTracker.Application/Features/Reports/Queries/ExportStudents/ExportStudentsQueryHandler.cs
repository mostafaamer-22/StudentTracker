using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Reports.DTOs;
using StudentTracker.Application.Specifications;
using StudentTracker.Domain.Repositories;
using StudentTracker.Domain.Shared;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Application.Features.Reports.Queries.ExportStudents;

internal sealed class ExportStudentsQueryHandler : IQueryHandler<ExportStudentsQuery, IEnumerable<StudentExportDto>>
{
    private readonly IGenericRepository<Student> _studentRepository;
    private readonly IGenericRepository<StudentProgress> _progressRepository;
    private readonly IGenericRepository<Assessment> _assessmentRepository;
    private readonly IGenericRepository<StudentCourse> _studentCourseRepository;
    private readonly IGenericRepository<Course> _courseRepository;
    private readonly IGenericRepository<Teacher> _teacherRepository;

    public ExportStudentsQueryHandler(
        IGenericRepository<Student> studentRepository,
        IGenericRepository<StudentProgress> progressRepository,
        IGenericRepository<Assessment> assessmentRepository,
        IGenericRepository<StudentCourse> studentCourseRepository,
        IGenericRepository<Course> courseRepository,
        IGenericRepository<Teacher> teacherRepository)
    {
        _studentRepository = studentRepository;
        _progressRepository = progressRepository;
        _assessmentRepository = assessmentRepository;
        _studentCourseRepository = studentCourseRepository;
        _courseRepository = courseRepository;
        _teacherRepository = teacherRepository;
    }

    public async Task<Result<IEnumerable<StudentExportDto>>> Handle(ExportStudentsQuery request, CancellationToken cancellationToken)
    {
        var studentsSpec = new StudentsForExportSpecification(
            request.Grade, request.Subject, request.TeacherId, request.StartDate, request.EndDate);
        var (studentQuery, _) = await _studentRepository.GetWithSpecAsync(studentsSpec, cancellationToken);
        var students = await studentQuery.ToListAsync(cancellationToken);

        if (!students.Any())
        {
            return Result.Success(Enumerable.Empty<StudentExportDto>());
        }

        var studentIds = students.Select(s => s.Id).ToList();

        var progressSpec = new StudentProgressForExportSpecification(studentIds);
        var (progressQuery, _) = await _progressRepository.GetWithSpecAsync(progressSpec, cancellationToken);
        var progressRecords = await progressQuery.ToListAsync(cancellationToken);

        var assessmentSpec = new AssessmentsForExportSpecification(studentIds);
        var (assessmentQuery, _) = await _assessmentRepository.GetWithSpecAsync(assessmentSpec, cancellationToken);
        var assessments = await assessmentQuery.ToListAsync(cancellationToken);

        var studentCourseSpec = new StudentCoursesForExportSpecification(studentIds);
        var (studentCourseQuery, _) = await _studentCourseRepository.GetWithSpecAsync(studentCourseSpec, cancellationToken);
        var studentCourses = await studentCourseQuery.ToListAsync(cancellationToken);

        var courseIds = studentCourses.Select(sc => sc.CourseId).Distinct().ToList();
        var courseSpec = new CoursesForExportSpecification(courseIds);
        var (courseQuery, _) = await _courseRepository.GetWithSpecAsync(courseSpec, cancellationToken);
        var courses = await courseQuery.ToListAsync(cancellationToken);

        var teacherIds = courses.Select(c => c.TeacherId).Distinct().ToList();
        var teacherSpec = new TeachersForExportSpecification(teacherIds);
        var (teacherQuery, _) = await _teacherRepository.GetWithSpecAsync(teacherSpec, cancellationToken);
        var teachers = await teacherQuery.ToListAsync(cancellationToken);

        var exportData = students.Select(student =>
        {
            var studentProgress = progressRecords.Where(p => p.StudentId == student.Id).ToList();
            var studentAssessments = assessments.Where(a => a.StudentId == student.Id).ToList();
            var studentCourseList = studentCourses.Where(sc => sc.StudentId == student.Id).ToList();
            var studentCourseDetails = courses.Where(c => studentCourseList.Any(sc => sc.CourseId == c.Id)).ToList();
            var studentTeachers = teachers.Where(t => studentCourseDetails.Any(c => c.TeacherId == t.Id)).ToList();

            var totalTimeSpentMinutes = studentProgress.Sum(p => p.TimeSpentMinutes);
            var lastActivity = studentProgress.Any() ?
                studentProgress.OrderByDescending(p => p.LastAccessedAt).First().LastAccessedAt :
                student.CreatedOnUtc;

            return new StudentExportDto
            {
                StudentId = student.Id.ToString(),
                FirstName = student.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = student.FullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Email = student.Email,
                Grade = student.Grade,
                DateOfBirth = student.DateOfBirth.ToString("yyyy-MM-dd"),
                EnrollmentDate = student.CreatedOnUtc.ToString("yyyy-MM-dd"),
                ParentEmail = student.ParentEmail ?? "",
                IsActive = student.IsActive ? "Yes" : "No",
                OverallCompletionPercentage = studentProgress.Any() ?
                    Math.Round(studentProgress.Average(p => p.CompletionPercentage), 2).ToString("F2") + "%" : "0.00%",
                AverageScore = studentAssessments.Any() ?
                    Math.Round(studentAssessments.Average(a => a.PercentageScore), 2).ToString("F2") + "%" : "N/A",
                TotalAssignments = studentProgress.Count.ToString(),
                CompletedAssignments = studentProgress.Count(p => p.Status == ProgressStatus.Completed).ToString(),
                OverdueAssignments = studentProgress.Count(p => p.IsOverdue).ToString(),
                TotalTimeSpentHours = Math.Round(totalTimeSpentMinutes / 60.0, 1).ToString("F1"),
                LastActivity = lastActivity.ToString("yyyy-MM-dd HH:mm:ss"),
                Courses = string.Join("; ", studentCourseDetails.Select(c => c.Name).OrderBy(n => n)),
                Teachers = string.Join("; ", studentTeachers.Select(t => t.FullName).OrderBy(n => n)),
                CreatedDate = student.CreatedOnUtc.ToString("yyyy-MM-dd"),
                LastModified = student.ModifiedOnUtc?.ToString("yyyy-MM-dd") ?? ""
            };
        }).ToList();

        return Result.Success(exportData.AsEnumerable());
    }
}