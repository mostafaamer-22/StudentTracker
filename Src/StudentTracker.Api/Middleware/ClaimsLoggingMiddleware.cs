using System.IdentityModel.Tokens.Jwt;

namespace StudentTracker.Api.Middleware;

public class ClaimsLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClaimsLoggingMiddleware> _logger;

    public ClaimsLoggingMiddleware(RequestDelegate next, ILogger<ClaimsLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("User is authenticated");

            // Log all claims
            foreach (var claim in context.User.Claims)
            {
                _logger.LogInformation("Claim: Type = {ClaimType}, Value = {ClaimValue}", claim.Type, claim.Value);
            }

            // Check role claims specifically
            var roleClaims = context.User.Claims.Where(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaims.Any())
            {
                _logger.LogInformation("Role claims found:");
                foreach (var roleClaim in roleClaims)
                {
                    _logger.LogInformation("Role Claim: Type = {ClaimType}, Value = {ClaimValue}", roleClaim.Type, roleClaim.Value);
                }
            }
            else
            {
                _logger.LogWarning("No role claims found!");
            }

            // Log authorization header
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    _logger.LogInformation("JWT Token claims:");
                    foreach (var claim in jwtToken.Claims)
                    {
                        _logger.LogInformation("JWT Claim: Type = {ClaimType}, Value = {ClaimValue}", claim.Type, claim.Value);
                    }
                }
            }

            // Log if user is in Teacher role
            var isInTeacherRole = context.User.IsInRole("Teacher");
            _logger.LogInformation("IsInRole('Teacher'): {IsInRole}", isInTeacherRole);
        }
        else
        {
            _logger.LogWarning("User is not authenticated");
        }

        await _next(context);
    }
}