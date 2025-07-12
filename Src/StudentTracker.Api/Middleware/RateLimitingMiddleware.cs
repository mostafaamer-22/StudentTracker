using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace StudentTracker.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, int> RequestCounts = new();
    private static readonly ConcurrentDictionary<string, DateTime> RequestTimestamps = new();
    private const int Limit = 100; // Max requests per time window
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (ipAddress == null)
        {
            await _next(context);
            return;
        }

        var now = DateTime.UtcNow;
        RequestTimestamps.AddOrUpdate(ipAddress, now, (key, oldValue) => now);

        if (RequestCounts.TryGetValue(ipAddress, out var count))
        {
            if (now - RequestTimestamps[ipAddress] < TimeWindow)
            {
                if (count >= Limit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                    return;
                }
                RequestCounts[ipAddress] = count + 1;
            }
            else
            {
                RequestCounts[ipAddress] = 1;
            }
        }
        else
        {
            RequestCounts[ipAddress] = 1;
        }

        await _next(context);
    }
}