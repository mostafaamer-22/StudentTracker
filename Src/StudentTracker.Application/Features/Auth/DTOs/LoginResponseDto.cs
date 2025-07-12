namespace StudentTracker.Application.Features.Auth.DTOs;

public record LoginResponseDto(
    string Token,
    string UserType,
    UserProfileDto Profile);

public record UserProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string UserType,
    Dictionary<string, object> AdditionalInfo);