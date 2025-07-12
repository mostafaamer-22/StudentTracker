using FluentAssertions;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using Xunit;

namespace StudentTracker.UnitTests.Domain.Entities;

public class StudentTests
{

    [Fact]
    public void Create_WithValidData_ShouldCreateStudent()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var grade = 10;
        var dateOfBirth = DateTime.Now.AddYears(-15);
        var parentEmail = "parent@example.com";

        // Act
        var student = Student.Create(firstName, lastName, email, grade, dateOfBirth, parentEmail);

        // Assert
        student.Should().NotBeNull();
        student.FullName.Should().Be("John Doe");
        student.Email.Should().Be(email.ToLowerInvariant());
        student.NormalizedEmail.Should().Be(email.ToUpperInvariant());
        student.Grade.Should().Be(grade);
        student.DateOfBirth.Should().Be(dateOfBirth.Date);
        student.ParentEmail.Should().Be(parentEmail);
        student.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithNullParentEmail_ShouldCreateStudentWithNullParentEmail()
    {
        // Arrange
        var firstName = "Jane";
        var lastName = "Smith";
        var email = "jane.smith@example.com";
        var grade = 8;
        var dateOfBirth = DateTime.Now.AddYears(-13);

        // Act
        var student = Student.Create(firstName, lastName, email, grade, dateOfBirth);

        // Assert
        student.Should().NotBeNull();
        student.ParentEmail.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldCreateStudentWithEmptyEmail()
    {
        // Arrange
        var firstName = "Test";
        var lastName = "User";
        var email = "";
        var grade = 5;
        var dateOfBirth = DateTime.Now.AddYears(-10);

        // Act
        var student = Student.Create(firstName, lastName, email, grade, dateOfBirth);

        // Assert
        student.Should().NotBeNull();
        student.Email.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, "Kindergarten")]
    [InlineData(1, "Grade 1")]
    [InlineData(5, "Grade 5")]
    [InlineData(12, "Grade 12")]
    [InlineData(13, "Unknown")]
    [InlineData(-1, "Unknown")]
    public void GradeDisplay_ShouldReturnCorrectDisplay(int grade, string expectedDisplay)
    {
        // Arrange
        var student = CreateTestStudent();
        student.SetGrade(grade);

        // Act
        var result = student.GradeDisplay;

        // Assert
        result.Should().Be(expectedDisplay);
    }

    [Fact]
    public void Age_ShouldCalculateCorrectAge()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-15).AddDays(-1);
        var student = CreateTestStudent();
        student.SetDateOfBirth(birthDate);

        // Act
        var age = student.Age;

        // Assert
        age.Should().Be(15);
    }

    [Fact]
    public void Age_BeforeBirthday_ShouldReturnCorrectAge()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-15).AddDays(1);
        var student = CreateTestStudent();
        student.SetDateOfBirth(birthDate);

        // Act
        var age = student.Age;

        // Assert
        age.Should().Be(14);
    }



    [Fact]
    public void SetGrade_ShouldUpdateGrade()
    {
        // Arrange
        var student = CreateTestStudent();
        var newGrade = 11;

        // Act
        student.SetGrade(newGrade);

        // Assert
        student.Grade.Should().Be(newGrade);
    }

    [Fact]
    public void SetDateOfBirth_ShouldUpdateDateOfBirth()
    {
        // Arrange
        var student = CreateTestStudent();
        var newDate = DateTime.Now.AddYears(-16).Date;

        // Act
        student.SetDateOfBirth(newDate);

        // Assert
        student.DateOfBirth.Should().Be(newDate);
    }

    [Fact]
    public void EnrollInCourse_ShouldAddCourseEnrollment()
    {
        // Arrange
        var student = CreateTestStudent();
        var course = CreateTestCourse();

        // Act
        student.EnrollInCourse(course);

        // Assert
        student.StudentCourses.Should().HaveCount(1);
        student.StudentCourses.First().StudentId.Should().Be(student.Id);
        student.StudentCourses.First().CourseId.Should().Be(course.Id);
    }

    [Fact]
    public void GetOverallCompletionPercentage_WithNoProgress_ShouldReturnZero()
    {
        // Arrange
        var student = CreateTestStudent();

        // Act
        var result = student.GetOverallCompletionPercentage();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetOverallCompletionPercentage_WithProgress_ShouldReturnAverage()
    {
        // Arrange
        var student = CreateTestStudent();
        var assignment1 = CreateTestAssignment();
        var assignment2 = CreateTestAssignment();

        var progress1 = CreateTestProgress(student.Id, assignment1.Id, 80.0m);
        var progress2 = CreateTestProgress(student.Id, assignment2.Id, 90.0m);

        student.ProgressRecords.Add(progress1);
        student.ProgressRecords.Add(progress2);

        // Act
        var result = student.GetOverallCompletionPercentage();

        // Assert
        result.Should().Be(85.0m);
    }

    [Fact]
    public void GetCompletedAssignmentsCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var student = CreateTestStudent();
        var assignment1 = CreateTestAssignment();
        var assignment2 = CreateTestAssignment();
        var assignment3 = CreateTestAssignment();

        var progress1 = CreateTestProgress(student.Id, assignment1.Id, 100.0m, ProgressStatus.Completed);
        var progress2 = CreateTestProgress(student.Id, assignment2.Id, 100.0m, ProgressStatus.Completed);
        var progress3 = CreateTestProgress(student.Id, assignment3.Id, 50.0m, ProgressStatus.InProgress);

        student.ProgressRecords.Add(progress1);
        student.ProgressRecords.Add(progress2);
        student.ProgressRecords.Add(progress3);

        // Act
        var result = student.GetCompletedAssignmentsCount();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetTotalAssignmentsCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var student = CreateTestStudent();
        var assignment1 = CreateTestAssignment();
        var assignment2 = CreateTestAssignment();

        var progress1 = CreateTestProgress(student.Id, assignment1.Id, 80.0m);
        var progress2 = CreateTestProgress(student.Id, assignment2.Id, 90.0m);

        student.ProgressRecords.Add(progress1);
        student.ProgressRecords.Add(progress2);

        // Act
        var result = student.GetTotalAssignmentsCount();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetAverageAssessmentScore_WithNoAssessments_ShouldReturnZero()
    {
        // Arrange
        var student = CreateTestStudent();

        // Act
        var result = student.GetAverageAssessmentScore();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetAverageAssessmentScore_WithAssessments_ShouldReturnAverage()
    {
        // Arrange
        var student = CreateTestStudent();
        var assignment1 = CreateTestAssignment();
        var assignment2 = CreateTestAssignment();

        var assessment1 = CreateTestAssessment(student.Id, assignment1.Id, 80.0m);
        var assessment2 = CreateTestAssessment(student.Id, assignment2.Id, 90.0m);

        student.Assessments.Add(assessment1);
        student.Assessments.Add(assessment2);

        // Act
        var result = student.GetAverageAssessmentScore();

        // Assert
        result.Should().Be(85.0m);
    }

    [Fact]
    public void GetRecentAssessments_ShouldReturnMostRecentAssessments()
    {
        // Arrange
        var student = CreateTestStudent();
        var assignment1 = CreateTestAssignment();
        var assignment2 = CreateTestAssignment();
        var assignment3 = CreateTestAssignment();

        var assessment1 = CreateTestAssessment(student.Id, assignment1.Id, 80.0m, DateTime.Now.AddDays(-3));
        var assessment2 = CreateTestAssessment(student.Id, assignment2.Id, 90.0m, DateTime.Now.AddDays(-1));
        var assessment3 = CreateTestAssessment(student.Id, assignment3.Id, 85.0m, DateTime.Now.AddDays(-2));

        student.Assessments.Add(assessment1);
        student.Assessments.Add(assessment2);
        student.Assessments.Add(assessment3);

        // Act
        var result = student.GetRecentAssessments(2);

        // Assert
        result.Should().HaveCount(2);
        result.First().Should().Be(assessment2); // Most recent
        result.Last().Should().Be(assessment3); // Second most recent
    }

    [Fact]
    public void IsActiveInCourse_WithActiveCourse_ShouldReturnTrue()
    {
        // Arrange
        var student = CreateTestStudent();
        var course = CreateTestCourse();
        var studentCourse = CreateTestStudentCourse(student.Id, course.Id, true);

        student.StudentCourses.Add(studentCourse);

        // Act
        var result = student.IsActiveInCourse(course.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActiveInCourse_WithInactiveCourse_ShouldReturnFalse()
    {
        // Arrange
        var student = CreateTestStudent();
        var course = CreateTestCourse();
        var studentCourse = CreateTestStudentCourse(student.Id, course.Id, false);

        student.StudentCourses.Add(studentCourse);

        // Act
        var result = student.IsActiveInCourse(course.Id);

        // Assert
        result.Should().BeFalse();
    }



    #region Helper Methods

    private Student CreateTestStudent()
    {
        return Student.Create("Test", "Student", "test@example.com", 10, DateTime.Now.AddYears(-15));
    }

    private Course CreateTestCourse()
    {
        return Course.Create("Test Course", "TEST101", "Test Description", 10, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(6));
    }

    private Assignment CreateTestAssignment(DateTime? dueDate = null)
    {
        return Assignment.Create(
            "Test Assignment",
            "Test Description",
            AssignmentType.Homework,
            100.0m,
            dueDate ?? DateTime.Now.AddDays(7),
            60,
            Guid.NewGuid());
    }

    private StudentProgress CreateTestProgress(Guid studentId, Guid assignmentId, decimal completionPercentage, ProgressStatus status = ProgressStatus.InProgress)
    {
        return StudentProgress.Create(
            completionPercentage,
            60,
            status,
            80.0m,
            DateTime.Now.AddDays(-1),
            status == ProgressStatus.Completed ? DateTime.Now : null,
            DateTime.Now,
            1,
            "Test notes",
            studentId,
            assignmentId);
    }

    private Assessment CreateTestAssessment(Guid studentId, Guid assignmentId, decimal percentageScore, DateTime? takenAt = null)
    {
        var maxScore = 100.0m;
        var score = (percentageScore / 100) * maxScore;

        return Assessment.Create(
            score,
            maxScore,
            AssessmentType.Quiz,
            takenAt ?? DateTime.Now,
            30,
            1,
            studentId,
            assignmentId);
    }

    private StudentCourse CreateTestStudentCourse(Guid studentId, Guid courseId, bool isActive)
    {
        var studentCourse = StudentCourse.Create(studentId, courseId);
        if (!isActive)
        {
            // Use reflection to set IsActive to false since there's no public setter
            var isActiveProperty = typeof(StudentCourse).GetProperty("IsActive");
            isActiveProperty?.SetValue(studentCourse, false);
        }
        return studentCourse;
    }

    #endregion
}