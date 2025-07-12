using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentTracker.Application.Features.Auth.DTOs;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Application.Features.Auth.Queries.GetUserProfile;

internal sealed class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly UserManager<User> _userManager;

    public GetUserProfileQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == request.UserId && u.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserProfileDto>(Error.NotFound("User not found"));
        }

        var userType = user switch
        {
            Student => "Student",
            Teacher => "Teacher",
            _ => "User"
        };

        var additionalInfo = new Dictionary<string, object>();

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        additionalInfo["roles"] = roles;

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

        return Result.Success(profile);
    }
}