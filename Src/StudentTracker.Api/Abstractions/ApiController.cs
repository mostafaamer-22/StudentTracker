
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Api.Abstractions;

[ApiController]
public class ApiController : ControllerBase
{
    protected readonly ISender Sender;

    protected ApiController(ISender sender) => Sender = sender;

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        else if (result.Error.Type is ErrorType.Validation)
        {
            return BadRequest(result);
        }

        return HandleError(result.Error);
    }

    protected IActionResult HandleResult<TValue>(Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        else if (result.Error.Type is ErrorType.Validation)
        {
            return BadRequest(result);
        }

        return HandleError(result.Error);
    }

    protected IActionResult HandleError(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            ErrorType.Validation => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.BadRequest => 400,
            ErrorType.InternalServerError => 500,
            ErrorType.ServiceUnavailable => 503,
            ErrorType.TooManyRequests => 429,
            ErrorType.UnprocessableEntity => 422,
            _ => 500
        };

        return Problem(statusCode: statusCode, title: error.Code, detail: error.Description);
    }
}
