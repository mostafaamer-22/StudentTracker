using FluentAssertions;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;
using Xunit;

namespace StudentTracker.UnitTests.Domain.Entities;

public class AssessmentTests
{


    [Fact]
    public void Create_WithNullFeedback_ShouldCreateAssessmentWithNullFeedback()
    {
        var score = 75m;
        var maxScore = 100m;
        var type = AssessmentType.Test;
        var takenAt = DateTime.Now;
        var timeSpentMinutes = 60;
        var attemptNumber = 1;
        var studentId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        var assessment = Assessment.Create(
            score, maxScore, type, takenAt, timeSpentMinutes,
            attemptNumber, studentId, assignmentId);

        assessment.Should().NotBeNull();
        assessment.Feedback.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceFeedback_ShouldTrimFeedback()
    {
        var score = 90m;
        var maxScore = 100m;
        var type = AssessmentType.Midterm;
        var takenAt = DateTime.Now;
        var timeSpentMinutes = 120;
        var attemptNumber = 1;
        var studentId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var feedback = "  Excellent performance!  ";

        var assessment = Assessment.Create(
            score, maxScore, type, takenAt, timeSpentMinutes,
            attemptNumber, studentId, assignmentId, feedback);

        assessment.Should().NotBeNull();
        assessment.Feedback.Should().Be("Excellent performance!");
    }

    [Theory]
    [InlineData(80, 100, 80.0)]
    [InlineData(85.5, 100, 85.5)]
    [InlineData(90, 120, 75.0)]
    [InlineData(0, 100, 0.0)]
    [InlineData(100, 100, 100.0)]
    public void PercentageScore_ShouldCalculateCorrectPercentage(decimal score, decimal maxScore, decimal expectedPercentage)
    {
        var assessment = CreateTestAssessment(score, maxScore);

        var percentageScore = assessment.PercentageScore;

        percentageScore.Should().Be(expectedPercentage);
    }

    [Fact]
    public void PercentageScore_WithZeroMaxScore_ShouldReturnZero()
    {
        var assessment = CreateTestAssessment(score: 50m, maxScore: 0m);

        var percentageScore = assessment.PercentageScore;

        percentageScore.Should().Be(0);
    }

    [Theory]
    [InlineData(70, 100, true)]
    [InlineData(85.5, 100, true)]
    [InlineData(69.9, 100, false)]
    [InlineData(0, 100, false)]
    [InlineData(100, 100, true)]
    [InlineData(84, 120, true)] 
    [InlineData(83, 120, false)]
    public void IsPassing_ShouldReturnCorrectValue(decimal score, decimal maxScore, bool expectedIsPassing)
    {
        var assessment = CreateTestAssessment(score, maxScore);

        var isPassing = assessment.IsPassing;

        isPassing.Should().Be(expectedIsPassing);
    }

    [Theory]
    [InlineData(AssessmentType.Quiz)]
    [InlineData(AssessmentType.Test)]
    [InlineData(AssessmentType.Midterm)]
    [InlineData(AssessmentType.Final)]
    [InlineData(AssessmentType.Practice)]
    [InlineData(AssessmentType.Diagnostic)]
    public void Create_WithDifferentTypes_ShouldCreateAssessmentWithCorrectType(AssessmentType type)
    {
        var assessment = CreateTestAssessment(type: type);

        assessment.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Create_WithDifferentAttemptNumbers_ShouldCreateAssessmentWithCorrectAttemptNumber(int attemptNumber)
    {
        var assessment = CreateTestAssessment(attemptNumber: attemptNumber);

        assessment.AttemptNumber.Should().Be(attemptNumber);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(240)]
    public void Create_WithDifferentTimeSpent_ShouldCreateAssessmentWithCorrectTimeSpent(int timeSpentMinutes)
    {
        var assessment = CreateTestAssessment(timeSpentMinutes: timeSpentMinutes);

        assessment.TimeSpentMinutes.Should().Be(timeSpentMinutes);
    }

    [Fact]
    public void Create_WithSpecificTakenAtTime_ShouldCreateAssessmentWithCorrectTakenAt()
    {
        var specificTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var assessment = CreateTestAssessment(takenAt: specificTime);

        assessment.TakenAt.Should().Be(specificTime);
    }

    [Fact]
    public void Create_WithDifferentStudentAndAssignment_ShouldCreateAssessmentWithCorrectIds()
    {
        var studentId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        var assessment = CreateTestAssessment(studentId: studentId, assignmentId: assignmentId);

        assessment.StudentId.Should().Be(studentId);
        assessment.AssignmentId.Should().Be(assignmentId);
    }

    [Fact]
    public void Create_WithDecimalScores_ShouldHandleDecimalPrecision()
    {
        var score = 87.75m;
        var maxScore = 100m;

        var assessment = CreateTestAssessment(score: score, maxScore: maxScore);

        assessment.Score.Should().Be(score);
        assessment.MaxScore.Should().Be(maxScore);
        assessment.PercentageScore.Should().Be(87.75m);
    }

    [Fact]
    public void Create_WithLargeScores_ShouldHandleLargeNumbers()
    {
        var score = 450m;
        var maxScore = 500m;

        var assessment = CreateTestAssessment(score: score, maxScore: maxScore);

        assessment.Score.Should().Be(score);
        assessment.MaxScore.Should().Be(maxScore);
        assessment.PercentageScore.Should().Be(90m);
    }

    [Fact]
    public void Create_WithZeroScore_ShouldCreateAssessmentWithZeroScore()
    {
        var score = 0m;
        var maxScore = 100m;

        var assessment = CreateTestAssessment(score: score, maxScore: maxScore);

        assessment.Score.Should().Be(0m);
        assessment.PercentageScore.Should().Be(0m);
        assessment.IsPassing.Should().BeFalse();
    }

    [Fact]
    public void Create_WithPerfectScore_ShouldCreateAssessmentWithPerfectScore()
    {
        var score = 100m;
        var maxScore = 100m;

        var assessment = CreateTestAssessment(score: score, maxScore: maxScore);

        assessment.Score.Should().Be(100m);
        assessment.PercentageScore.Should().Be(100m);
        assessment.IsPassing.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyStringFeedback_ShouldTrimToEmptyString()
    {
        var feedback = "";

        var assessment = CreateTestAssessment(feedback: feedback);

        assessment.Feedback.Should().Be("");
    }

    [Fact]
    public void Create_WithOnlyWhitespaceFeedback_ShouldTrimToEmptyString()
    {
        var feedback = "   ";

        var assessment = CreateTestAssessment(feedback: feedback);

        assessment.Feedback.Should().Be("");
    }

    #region Helper Methods

    private Assessment CreateTestAssessment(
        decimal score = 85m,
        decimal maxScore = 100m,
        AssessmentType type = AssessmentType.Quiz,
        DateTime? takenAt = null,
        int timeSpentMinutes = 45,
        int attemptNumber = 1,
        Guid? studentId = null,
        Guid? assignmentId = null,
        string? feedback = null)
    {
        return Assessment.Create(
            score,
            maxScore,
            type,
            takenAt ?? DateTime.Now,
            timeSpentMinutes,
            attemptNumber,
            studentId ?? Guid.NewGuid(),
            assignmentId ?? Guid.NewGuid(),
            feedback);
    }

    #endregion
}