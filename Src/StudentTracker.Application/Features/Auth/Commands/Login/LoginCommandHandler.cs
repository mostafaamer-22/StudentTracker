using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Abstractions.Services;
using StudentTracker.Application.Features.Auth.DTOs;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        UserManager<User> userManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .OfType<User>()
            .Where(u => u.Email == request.Email.ToLowerInvariant() && u.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result.Failure<LoginResponseDto>(Error.NotFound("Invalid email or password"));
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!result)
            return Result.Failure<LoginResponseDto>(Error.NotFound("Invalid email or password"));


        user.RecordLogin();
        await _userManager.UpdateAsync(user);

        var token = await _jwtService.GenerateToken(user);

        var userType = user switch
        {
            Student => "Student",
            Teacher => "Teacher",
            _ => "User"
        };

        var additionalInfo = new Dictionary<string, object>();

        if (user is Student student)
        {
            additionalInfo["grade"] = student.Grade;
            additionalInfo["gradeDisplay"] = student.GradeDisplay;
            additionalInfo["age"] = student.Age;
            additionalInfo["isMinor"] = student.IsMinor;
            if (!string.IsNullOrEmpty(student.ParentEmail))
                additionalInfo["parentEmail"] = student.ParentEmail;
        }
        else if (user is Teacher teacher)
        {
            additionalInfo["department"] = teacher.Department ?? "Not specified";
            additionalInfo["qualifications"] = teacher.Qualifications ?? "Not specified";
            additionalInfo["activeCoursesCount"] = teacher.ActiveCoursesCount;
            additionalInfo["activeStudentsCount"] = teacher.ActiveStudentsCount;
        }

        var profile = new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            userType,
            additionalInfo);

        var response = new LoginResponseDto(token, userType, profile);

        return Result.Success(response);
    }
}