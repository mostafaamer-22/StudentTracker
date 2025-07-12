using Microsoft.AspNetCore.Identity;
using StudentTracker.Domain.Primitives;
using StudentTracker.Domain.Shared;

namespace StudentTracker.Domain.Entities;

public class User : IdentityUser<Guid>, IAuditableEntity
{
    protected User() { }

    public bool IsActive { get; protected set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string FullName { get; protected set; }
    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);
    public TimeSpan? TimeSinceLastLogin => LastLoginAt.HasValue ? DateTime.UtcNow - LastLoginAt.Value : null;

    public static User create(string firstName, string lastName, string email)
    {
        return new User
        {
            Email = email?.Trim().ToLowerInvariant() ?? string.Empty,
            UserName = email!.Split('@')[0],
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = (email!.Split('@')[0]).ToUpperInvariant(),
            IsActive = true,
            FullName = $"{firstName} {lastName}".Trim(),
            Id = Guid.NewGuid()
        };

    }
    public void SetProfileImage(string? imageUrl)
    {
        ProfileImageUrl = imageUrl?.Trim();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;

    }

}