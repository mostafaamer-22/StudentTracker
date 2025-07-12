using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StudentTracker.Application.Abstractions.DataSeeder;

namespace StudentTracker.Infrastructure.Services.Seeders;

internal sealed class RoleSeeder : ISeeder, IRoleSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<RoleSeeder> _logger;

    public int ExecutionOrder { get; set; } = -1; 

    public RoleSeeder(RoleManager<IdentityRole<Guid>> roleManager, ILogger<RoleSeeder> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
    }

    public async Task SeedRolesAsync()
    {
        var roles = new[] { "Teacher", "Student" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role {Role} created successfully.", role);
                }
                else
                {
                    _logger.LogError("Failed to create role {Role}: {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogInformation("Role {Role} already exists.", role);
            }
        }
    }
}