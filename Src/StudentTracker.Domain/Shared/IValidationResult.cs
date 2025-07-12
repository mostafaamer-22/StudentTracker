namespace StudentTracker.Domain.Shared;
public interface IValidationResult
{
    string[] PropertyNames { get; }
    string[] ErrorMessages { get; }
}
