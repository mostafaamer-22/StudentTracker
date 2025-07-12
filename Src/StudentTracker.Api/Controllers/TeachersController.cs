using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Api.Abstractions;
using StudentTracker.Application.Features.Students.DTOs;
using StudentTracker.Application.Features.Teachers.Queries.GetTeacherStudents;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Api.Controllers;

[Authorize(Roles = "Teacher")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TeachersController : ApiController
{
    public TeachersController(ISender sender) : base(sender)
    {
    }

    [HttpGet("{id:guid}/students")]
    [ProducesResponseType(typeof(Pagination<GetStudentsForSpecificalTeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeacherStudents(
        [FromRoute] Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {

        var query = new GetTeacherStudentsQuery(id, page, pageSize, searchTerm);
        var result = await Sender.Send(query, cancellationToken);

        return HandleResult(result);
    }
}