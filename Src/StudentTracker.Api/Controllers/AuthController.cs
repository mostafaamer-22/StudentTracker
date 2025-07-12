using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Api.Abstractions;
using StudentTracker.Application.Features.Auth.Commands.Login;
using StudentTracker.Application.Features.Auth.DTOs;
using StudentTracker.Application.Features.Auth.Queries.GetUserProfile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace StudentTracker.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ApiController
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISender sender, ILogger<AuthController> logger) : base(sender)
    {
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required");
        }

        var command = new LoginCommand(request.Email, request.Password);
        var result = await Sender.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized("Invalid token");
        }

        var query = new GetUserProfileQuery(parsedUserId);
        var result = await Sender.Send(query, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("test-auth")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult TestAuth()
    {
        _logger.LogInformation("Test auth endpoint called");

        if (User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("User is authenticated");
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            _logger.LogInformation("Claims: {@Claims}", claims);

            var roles = User.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
            _logger.LogInformation("Roles: {@Roles}", roles);

            var isInTeacherRole = User.IsInRole("Teacher");
            _logger.LogInformation("IsInRole('Teacher'): {IsInRole}", isInTeacherRole);

            return Ok(new
            {
                IsAuthenticated = true,
                Username = User.Identity.Name,
                Claims = claims,
                Roles = roles,
                IsTeacher = isInTeacherRole
            });
        }
        else
        {
            _logger.LogWarning("User is not authenticated");
            return Unauthorized(new { message = "Not authenticated" });
        }
    }
}