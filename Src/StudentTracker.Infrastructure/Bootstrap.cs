using Bogus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StudentTracker.Application.Abstractions.Caching;
using StudentTracker.Application.Abstractions.DataSeeder;
using StudentTracker.Application.Abstractions.Services;
using StudentTracker.Application.Abstractions.Token;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Repositories;
using StudentTracker.Infrastructure.Interceptors;
using StudentTracker.Infrastructure.Options;
using StudentTracker.Infrastructure.Repositories;
using StudentTracker.Infrastructure.Services.Auth;
using StudentTracker.Infrastructure.Services.Caching;
using StudentTracker.Infrastructure.Services.Seeders;
using StudentTracker.Infrastructure.Services.Token;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentTracker.Infrastructure;
public static class Bootstrap
{
    public static async Task<IServiceCollection> AddInfrastructureStrapping(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenExtractor, TokenExtractor>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddMemoryCache();
        services.AddAuth(configuration);

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddDbConfig(configuration);

        services.AddScoped<ISeeder, RoleSeeder>();         // ExecutionOrder = -1
        services.AddScoped<ISeeder, CourseSeeder>();       // ExecutionOrder = 0
        services.AddScoped<ISeeder, AssignmentSeeder>();   // ExecutionOrder = 1
        services.AddScoped<ISeeder, StudentSeeder>();      // ExecutionOrder = 2
        services.AddScoped<ISeeder, StudentProgressSeeder>();// ExecutionOrder = 3
        services.AddScoped<IRoleSeeder, RoleSeeder>();

        services.AddSingleton<DatabaseInitializer>();

        var serviceProvider = services.BuildServiceProvider();

        await serviceProvider.GetRequiredService<DatabaseInitializer>().InitializeDatabaseAsync();
        await serviceProvider.GetRequiredService<DatabaseInitializer>().SeedDatabaseAsync();

        return services;
    }

    private static IServiceCollection AddDbConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.User.RequireUniqueEmail = true;
        })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        string connectionString = configuration.GetConnectionString("Database")!;

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A valid database connection string must be provided.");

        services.AddSingleton<SoftDeleteEntitiesInterceptor>();
        services.AddSingleton<AuditableEntitiesInterceptor>();
        services.AddScoped<DomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>(
            (sp, optionsBuilder) =>
            {
                optionsBuilder.UseSqlServer(connectionString);
                optionsBuilder.AddInterceptors(sp.GetRequiredService<SoftDeleteEntitiesInterceptor>());
                optionsBuilder.AddInterceptors(sp.GetRequiredService<AuditableEntitiesInterceptor>());
                optionsBuilder.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

        return services;
    }

    private static IServiceCollection AddAuth(
          this IServiceCollection services,
          IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        return services;
    }
}
