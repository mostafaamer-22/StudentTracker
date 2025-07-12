using Microsoft.AspNetCore.Diagnostics;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Api.Middleware;

public sealed class GlobalExceptionHandlerMiddleware : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception, "Exception occurred: {Message}", exception.Message);

        var response = new Result(false, Error.InternalServerError(exception.Message));

        await httpContext.Response
            .WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
