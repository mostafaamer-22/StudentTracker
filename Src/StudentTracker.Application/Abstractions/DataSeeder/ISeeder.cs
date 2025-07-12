namespace StudentTracker.Application.Abstractions.DataSeeder;

public interface ISeeder
{
    public int ExecutionOrder { get; set; }
    Task SeedAsync();
}
