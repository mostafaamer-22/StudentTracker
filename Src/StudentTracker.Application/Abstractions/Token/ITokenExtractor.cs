namespace StudentTracker.Application.Abstractions.Token;

public interface ITokenExtractor
{
    Guid? GetCurrentUserId();
    string? GetCurrentUserEmail();
    string? GetCurrentUserRole();
    List<string> GetCurrentUserPermissions();
    bool HasPermission(string permission);
    bool IsAuthenticated();
}
