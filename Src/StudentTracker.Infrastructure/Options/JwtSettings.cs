namespace StudentTracker.Infrastructure.Options;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}
