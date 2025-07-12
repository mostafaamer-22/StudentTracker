using Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Auth.DTOs;

namespace StudentTracker.Application.Features.Auth.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IQuery<UserProfileDto>;