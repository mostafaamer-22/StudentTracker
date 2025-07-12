namespace StudentTracker.Domain.Shared;
public sealed class ValidationResult : Result, IValidationResult
{
    private ValidationResult(string[] props, string[] errors)
        : base(false, Error.Validation())
    {
        PropertyNames = props;
        ErrorMessages = errors;
    }

    public string[] ErrorMessages { get; }

    public string[] PropertyNames { get; }

    public static ValidationResult WithErrors(string[] props, string[] errors) => new ValidationResult(props, errors);
}
