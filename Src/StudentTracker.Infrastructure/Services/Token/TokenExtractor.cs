using Microsoft.AspNetCore.Http;
using StudentTracker.Application.Abstractions.Token;
using System.Security.Claims;

namespace StudentTracker.Infrastructure.Services.Token;

public class TokenExtractor : ITokenExtractor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenExtractor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value ??
                         _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public string? GetCurrentUserRole()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value ??
               _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
    }

    public List<string> GetCurrentUserPermissions()
    {
        return _httpContextAccessor.HttpContext?.User?.FindAll("permission")
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
    }

    public bool HasPermission(string permission)
    {
        var permissions = GetCurrentUserPermissions();
        return permissions.Contains(permission);
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}