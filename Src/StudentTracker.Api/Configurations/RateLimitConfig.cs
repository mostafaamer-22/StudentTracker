using Microsoft.AspNetCore.RateLimiting;

namespace StudentTracker.Api.Configurations;

internal static class RateLimitConfig
{
    internal static IServiceCollection AddRateLimitConfig(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("AuthPolicy", rateLimiterOptions =>
            {
                rateLimiterOptions.Window = TimeSpan.FromMinutes(1);
                rateLimiterOptions.PermitLimit = 5; 
                rateLimiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            });

            options.AddFixedWindowLimiter("RefreshTokenPolicy", rateLimiterOptions =>
            {
                rateLimiterOptions.Window = TimeSpan.FromMinutes(1);
                rateLimiterOptions.PermitLimit = 10; 
                rateLimiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            });

            options.AddFixedWindowLimiter("ForgotPasswordPolicy", rateLimiterOptions =>
            {
                rateLimiterOptions.Window = TimeSpan.FromMinutes(15);
                rateLimiterOptions.PermitLimit = 3; 
                rateLimiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                if (context.Lease.TryGetMetadata(System.Threading.RateLimiting.MetadataName.RetryAfter, out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync(
                        $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s).", token);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.", token);
                }
            };
        });

        return services;
    }
}
