using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Api.Abstractions;
using StudentTracker.Application.Features.Reports.DTOs;
using StudentTracker.Application.Features.Reports.Queries.ExportStudents;
using System.Text;

namespace StudentTracker.Api.Controllers;
[Authorize(Roles = "Teacher")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReportsController : ApiController
{
    public ReportsController(ISender sender) : base(sender)
    {
    }

    [HttpGet("student-export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportStudents(
        [FromQuery] int? grade = null,
        [FromQuery] string? subject = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        if (format.ToLower() != "csv")
        {
            return BadRequest("Only CSV format is currently supported");
        }

        var query = new ExportStudentsQuery(grade, subject, teacherId, startDate, endDate);
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            var csv = GenerateCsvContent(result.Value);
            var fileName = $"students_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            return File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv",
                fileName);
        }

        return HandleResult(result);
    }

    private static string GenerateCsvContent(IEnumerable<StudentExportDto> students)
    {
        var csv = new StringBuilder();

        csv.AppendLine("Student ID,First Name,Last Name,Email,Grade,Date of Birth,Enrollment Date,Parent Email,Is Active,Overall Completion %,Average Score,Total Assignments,Completed Assignments,Overdue Assignments,Total Time Spent (Hours),Last Activity,Courses,Teachers,Created Date,Last Modified");

        foreach (var student in students)
        {
            csv.AppendLine($"{EscapeCsvField(student.StudentId)},{EscapeCsvField(student.FirstName)},{EscapeCsvField(student.LastName)},{EscapeCsvField(student.Email)},{student.Grade},{EscapeCsvField(student.DateOfBirth)},{EscapeCsvField(student.EnrollmentDate)},{EscapeCsvField(student.ParentEmail)},{EscapeCsvField(student.IsActive)},{EscapeCsvField(student.OverallCompletionPercentage)},{EscapeCsvField(student.AverageScore)},{EscapeCsvField(student.TotalAssignments)},{EscapeCsvField(student.CompletedAssignments)},{EscapeCsvField(student.OverdueAssignments)},{EscapeCsvField(student.TotalTimeSpentHours)},{EscapeCsvField(student.LastActivity)},{EscapeCsvField(student.Courses)},{EscapeCsvField(student.Teachers)},{EscapeCsvField(student.CreatedDate)},{EscapeCsvField(student.LastModified)}");
        }

        return csv.ToString();
    }

    private static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}