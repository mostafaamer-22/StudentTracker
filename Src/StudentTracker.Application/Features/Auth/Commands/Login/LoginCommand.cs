using StudentTracker.Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Auth.DTOs;

namespace StudentTracker.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : ICommand<LoginResponseDto>;