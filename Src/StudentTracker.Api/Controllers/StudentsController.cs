using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Api.Abstractions;
using StudentTracker.Application.Features.Students.Commands.UpdateStudentProgress;
using StudentTracker.Application.Features.Students.DTOs;
using StudentTracker.Application.Features.Students.Queries.GetStudentById;
using StudentTracker.Application.Features.Students.Queries.GetStudentProgress;
using StudentTracker.Application.Features.Students.Queries.GetStudents;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Api.Controllers;
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class StudentsController : ApiController
{
    public StudentsController(ISender sender) : base(sender)
    {
    }
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStudents([FromQuery] GetStudentsQuery query)
    {

        var result = await Sender.Send(query);

        Response.Headers.Append("X-Pagination-CurrentPage", result.CurrentPage.ToString());
        Response.Headers.Append("X-Pagination-PageSize", result.PageSize.ToString());
        Response.Headers.Append("X-Pagination-TotalPages", result.TotalPages.ToString());
        Response.Headers.Append("X-Pagination-TotalItems", result.TotalItems.ToString());
        Response.Headers.Append("X-Pagination-HasPrevious", result.HasPreviousPage.ToString());
        Response.Headers.Append("X-Pagination-HasNext", result.HasNextPage.ToString());

        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        var query = new GetStudentByIdQuery(id);
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentProgress(
        Guid id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetStudentProgressQuery(id, startDate, endDate);
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStudentProgress(
        Guid id,
        [FromBody] UpdateProgressRequestDto request)
    {
        var command = new UpdateStudentProgressCommand(
            id,
            request.AssignmentId,
            request.CompletionPercentage,
            request.TimeSpentMinutes,
            request.Status,
            request.Status == ProgressStatus.Completed ? request.CompletionPercentage : null,
            DateTime.UtcNow,
            request.Status == ProgressStatus.Completed ? DateTime.UtcNow : null,
            DateTime.UtcNow,
            1,
            request.Notes);

        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}