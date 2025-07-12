namespace StudentTracker.Application.Features.Auth.DTOs;

public record LoginRequestDto(
    string Email,
    string Password);