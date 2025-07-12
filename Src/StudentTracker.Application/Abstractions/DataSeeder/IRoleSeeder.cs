using System.Threading.Tasks;

namespace StudentTracker.Application.Abstractions.DataSeeder;

public interface IRoleSeeder
{
    Task SeedRolesAsync();
}