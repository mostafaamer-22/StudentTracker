using FluentAssertions;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using System.Reflection;
using Xunit;

namespace StudentTracker.UnitTests.Domain.Entities;

public class CourseTests
{


    [Fact]
    public void Create_WithDefaultTotalPoints_ShouldCreateCourseWithDefaultPoints()
    {
        // Arrange
        var name = "English Literature";
        var code = "ENG201";
        var description = "English literature course";
        var gradeLevel = 11;
        var teacherId = Guid.NewGuid();
        var startDate = DateTime.Now.AddDays(1);
        var endDate = DateTime.Now.AddMonths(6);

        // Act
        var course = Course.Create(name, code, description, gradeLevel, teacherId, startDate, endDate);

        // Assert
        course.Should().NotBeNull();
        course.TotalPoints.Should().Be(100m);
    }

    [Fact]
    public void Duration_ShouldCalculateCorrectDuration()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = DateTime.Now.AddDays(181); // 180 days later
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var duration = course.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromDays(180));
    }

    [Fact]
    public void IsCurrentlyActive_WithActiveAndInDateRange_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var isCurrentlyActive = course.IsCurrentlyActive;

        // Assert
        isCurrentlyActive.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentlyActive_WithActiveButNotStarted_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var isCurrentlyActive = course.IsCurrentlyActive;

        // Assert
        isCurrentlyActive.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentlyActive_WithActiveButEnded_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-1);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var isCurrentlyActive = course.IsCurrentlyActive;

        // Assert
        isCurrentlyActive.Should().BeFalse();
    }

    [Fact]
    public void HasStarted_WithStartDateInPast_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var hasStarted = course.HasStarted;

        // Assert
        hasStarted.Should().BeTrue();
    }

    [Fact]
    public void HasStarted_WithStartDateInFuture_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var hasStarted = course.HasStarted;

        // Assert
        hasStarted.Should().BeFalse();
    }

    [Fact]
    public void HasEnded_WithEndDateInPast_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-1);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var hasEnded = course.HasEnded;

        // Assert
        hasEnded.Should().BeTrue();
    }

    [Fact]
    public void HasEnded_WithEndDateInFuture_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var hasEnded = course.HasEnded;

        // Assert
        hasEnded.Should().BeFalse();
    }

    [Fact]
    public void IsUpcoming_WithStartDateInFuture_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var isUpcoming = course.IsUpcoming;

        // Assert
        isUpcoming.Should().BeTrue();
    }

    [Fact]
    public void IsUpcoming_WithStartDateInPast_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var course = CreateTestCourse(startDate: startDate, endDate: endDate);

        // Act
        var isUpcoming = course.IsUpcoming;

        // Assert
        isUpcoming.Should().BeFalse();
    }

    [Fact]
    public void EnrollStudent_ShouldAddStudentToCourse()
    {
        // Arrange
        var course = CreateTestCourse();
        var student = CreateTestStudent();

        // Act
        course.EnrollStudent(student);

        // Assert
        course.StudentCourses.Should().HaveCount(1);
        course.StudentCourses.First().StudentId.Should().Be(student.Id);
        course.StudentCourses.First().CourseId.Should().Be(course.Id);
    }

    [Fact]
    public void AddAssignment_ShouldAddAssignmentToCourse()
    {
        // Arrange
        var course = CreateTestCourse();
        var assignment = CreateTestAssignment(course.Id);

        // Act
        course.AddAssignment(assignment);

        // Assert
        course.Assignments.Should().HaveCount(1);
        course.Assignments.First().Should().Be(assignment);
    }


    [Fact]
    public void TotalAssignmentsCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var course = CreateTestCourse();
        var assignment1 = CreateTestAssignment(course.Id, isActive: true);
        var assignment2 = CreateTestAssignment(course.Id, isActive: true);
        var assignment3 = CreateTestAssignment(course.Id, isActive: false);

        course.Assignments.Add(assignment1);
        course.Assignments.Add(assignment2);
        course.Assignments.Add(assignment3);

        // Act
        var count = course.TotalAssignmentsCount;

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void TotalAssignmentPoints_ShouldReturnCorrectSum()
    {
        // Arrange
        var course = CreateTestCourse();
        var assignment1 = CreateTestAssignment(course.Id, maxPoints: 100m, isActive: true);
        var assignment2 = CreateTestAssignment(course.Id, maxPoints: 150m, isActive: true);
        var assignment3 = CreateTestAssignment(course.Id, maxPoints: 200m, isActive: false);

        course.Assignments.Add(assignment1);
        course.Assignments.Add(assignment2);
        course.Assignments.Add(assignment3);

        // Act
        var totalPoints = course.TotalAssignmentPoints;

        // Assert
        totalPoints.Should().Be(250m);
    }

    [Fact]
    public void GetActiveAssignments_ShouldReturnOnlyActiveAssignments()
    {
        // Arrange
        var course = CreateTestCourse();
        var activeAssignment1 = CreateTestAssignment(course.Id, isActive: true);
        var activeAssignment2 = CreateTestAssignment(course.Id, isActive: true);
        var inactiveAssignment = CreateTestAssignment(course.Id, isActive: false);

        course.Assignments.Add(activeAssignment1);
        course.Assignments.Add(activeAssignment2);
        course.Assignments.Add(inactiveAssignment);

        // Act
        var activeAssignments = course.GetActiveAssignments();

        // Assert
        activeAssignments.Should().HaveCount(2);
        activeAssignments.Should().Contain(activeAssignment1);
        activeAssignments.Should().Contain(activeAssignment2);
        activeAssignments.Should().NotContain(inactiveAssignment);
    }

    [Fact]
    public void GetOverdueAssignments_ShouldReturnOnlyOverdueAssignments()
    {
        // Arrange
        var course = CreateTestCourse();
        var overdueAssignment1 = CreateTestAssignment(course.Id, dueDate: DateTime.UtcNow.AddDays(-1), isActive: true);
        var overdueAssignment2 = CreateTestAssignment(course.Id, dueDate: DateTime.UtcNow.AddDays(-2), isActive: true);
        var futureAssignment = CreateTestAssignment(course.Id, dueDate: DateTime.UtcNow.AddDays(1), isActive: true);
        var inactiveOverdueAssignment = CreateTestAssignment(course.Id, dueDate: DateTime.UtcNow.AddDays(-1), isActive: false);

        course.Assignments.Add(overdueAssignment1);
        course.Assignments.Add(overdueAssignment2);
        course.Assignments.Add(futureAssignment);
        course.Assignments.Add(inactiveOverdueAssignment);

        // Act
        var overdueAssignments = course.GetOverdueAssignments();

        // Assert
        overdueAssignments.Should().HaveCount(2);
        overdueAssignments.Should().Contain(overdueAssignment1);
        overdueAssignments.Should().Contain(overdueAssignment2);
        overdueAssignments.Should().NotContain(futureAssignment);
        overdueAssignments.Should().NotContain(inactiveOverdueAssignment);
    }

    [Fact]
    public void CanAcceptNewEnrollments_WithActiveAndNotEnded_ShouldReturnTrue()
    {
        // Arrange
        var course = CreateTestCourse(
            startDate: DateTime.UtcNow.AddDays(-1),
            endDate: DateTime.UtcNow.AddDays(30));

        // Act
        var canAccept = course.CanAcceptNewEnrollments();

        // Assert
        canAccept.Should().BeTrue();
    }

    [Fact]
    public void CanAcceptNewEnrollments_WithActiveButEnded_ShouldReturnFalse()
    {
        // Arrange
        var course = CreateTestCourse(
            startDate: DateTime.UtcNow.AddDays(-30),
            endDate: DateTime.UtcNow.AddDays(-1));

        // Act
        var canAccept = course.CanAcceptNewEnrollments();

        // Assert
        canAccept.Should().BeFalse();
    }

    [Fact]
    public void IsStudentEnrolled_WithActiveEnrollment_ShouldReturnTrue()
    {
        // Arrange
        var course = CreateTestCourse();
        var student = CreateTestStudent();
        var enrollment = CreateTestStudentCourse(student.Id, course.Id, true);

        course.StudentCourses.Add(enrollment);

        // Act
        var isEnrolled = course.IsStudentEnrolled(student.Id);

        // Assert
        isEnrolled.Should().BeTrue();
    }


    [Fact]
    public void IsStudentEnrolled_WithNoEnrollment_ShouldReturnFalse()
    {
        // Arrange
        var course = CreateTestCourse();
        var studentId = Guid.NewGuid();

        // Act
        var isEnrolled = course.IsStudentEnrolled(studentId);

        // Assert
        isEnrolled.Should().BeFalse();
    }

    #region Helper Methods

    private Course CreateTestCourse(
        string name = "Test Course",
        string code = "TEST101",
        string description = "Test Description",
        int gradeLevel = 10,
        Guid? teacherId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal totalPoints = 100m)
    {
        return Course.Create(
            name,
            code,
            description,
            gradeLevel,
            teacherId ?? Guid.NewGuid(),
            startDate ?? DateTime.Now,
            endDate ?? DateTime.Now.AddMonths(6),
            totalPoints);
    }

    private Student CreateTestStudent(bool isActive = true)
    {
        var student = Student.Create("Test", "Student", "test@example.com", 10, DateTime.Now.AddYears(-15));
        if (!isActive)
        {
            student.Deactivate();
        }
        return student;
    }

    private Assignment CreateTestAssignment(Guid courseId, DateTime? dueDate = null, decimal maxPoints = 100m, bool isActive = true)
    {
        var assignment = Assignment.Create(
            "Test Assignment",
            "Test Description",
            AssignmentType.Homework,
            maxPoints,
            dueDate ?? DateTime.Now.AddDays(7),
            60,
            courseId);

        if (!isActive)
        {
            var isActiveProperty = typeof(Assignment).GetProperty("IsActive");
            isActiveProperty?.SetValue(assignment, false);
        }

        return assignment;
    }

    private StudentCourse CreateTestStudentCourse(Guid studentId, Guid courseId, bool isActive)
    {
        var studentCourse = StudentCourse.Create(studentId, courseId);
        if (!isActive)
        {
            var isActiveProperty = typeof(StudentCourse).GetProperty("IsActive", BindingFlags.NonPublic | BindingFlags.Instance);
            isActiveProperty?.SetValue(studentCourse, false);
        }

        return studentCourse;
    }

    #endregion
}