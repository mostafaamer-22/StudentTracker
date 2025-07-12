using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Api.Abstractions;
using StudentTracker.Application.Features.Analytics.Queries.GetClassSummary;
using StudentTracker.Application.Features.Analytics.Queries.GetProgressTrends;

namespace StudentTracker.Api.Controllers;
[Authorize(Roles = "Teacher")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AnalyticsController : ApiController
{
    public AnalyticsController(ISender sender) : base(sender)
    {
    }

    [HttpGet("class-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetClassSummary(
        [FromQuery] int? grade = null,
        [FromQuery] string? subject = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetClassSummaryQuery(grade, subject, teacherId, startDate, endDate);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("progress-trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProgressTrends(
        [FromQuery] int? grade = null,
        [FromQuery] string? subject = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string period = "weekly",
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgressTrendsQuery(grade, subject, teacherId, startDate, endDate, period);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result);
    }
}