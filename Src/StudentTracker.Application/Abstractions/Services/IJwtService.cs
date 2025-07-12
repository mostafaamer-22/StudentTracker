using StudentTracker.Domain.Entities;

namespace StudentTracker.Application.Abstractions.Services;

public interface IJwtService
{
    Task<string> GenerateToken(User user);
    Guid? GetUserIdFromToken(string token);
    bool ValidateToken(string token);
}