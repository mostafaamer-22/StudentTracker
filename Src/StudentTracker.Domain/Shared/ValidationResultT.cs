namespace StudentTracker.Domain.Shared;
public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    private ValidationResult(TValue value, string[] props, string[] errors)
        : base(value, false, Error.Validation())
    {
        Value = value;
        ErrorMessages = errors;
        PropertyNames = props;
    }

    public TValue Value { get; }
    public string[] ErrorMessages { get; }
    public string[] PropertyNames { get; }

    public static ValidationResult<TValue> WithErrors(TValue value, string[] props, string[] errors)
        => new ValidationResult<TValue>(value, props, errors);
}
