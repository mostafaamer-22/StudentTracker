using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudentTracker.Application.Abstractions.DataSeeder;
using System.Data;

namespace StudentTracker.Infrastructure;

internal class DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeDatabaseAsync()
    {
        _logger.LogInformation("Starting database initialization");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            bool dbExists = await dbContext.Database.CanConnectAsync();

            if (!dbExists)
            {
                _logger.LogInformation("Database does not exist. Creating database and applying migrations");
                await dbContext.Database.MigrateAsync();
                _logger.LogInformation("Database created and migrations applied successfully");
            }
            else
            {
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var pendingMigrationsList = pendingMigrations.ToList();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Pending migrations found: {Migrations}", string.Join(", ", pendingMigrationsList));
                    await dbContext.Database.MigrateAsync();
                }
                else
                {
                    _logger.LogInformation("Database is up to date. No migrations to apply");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database initialization");
            throw;
        }
    }
    public async Task SeedDatabaseAsync()
    {
        _logger.LogInformation("Starting database seeding");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            var seeders = scopedProvider.GetServices<ISeeder>()
                .OrderBy(x => x.ExecutionOrder);

            foreach (var seeder in seeders)
            {
                _logger.LogInformation("Running seeder: {SeederType}", seeder.GetType().Name);
                await seeder.SeedAsync();
            }

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database seeding");
            throw;
        }
    }
}
