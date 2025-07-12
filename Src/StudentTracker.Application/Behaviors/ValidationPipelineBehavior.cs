using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using StudentTracker.Domain.Shared;

namespace Application.Behaviors;

public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
       where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationPipelineBehavior<TRequest, TResponse>> _logger;

    public ValidationPipelineBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationPipelineBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var validationTasks = _validators
            .Select(validator => validator.ValidateAsync(request, cancellationToken))
            .ToList();

        await Task.WhenAll(validationTasks);

        var validationResults = validationTasks
            .SelectMany(validationTask => validationTask.Result.Errors)
            .Where(validationFailure => validationFailure != null)
            .Distinct();

        var propertyNames = validationResults
            .Select(failure => failure.PropertyName)
            .ToArray();

        var errorMessages = validationResults
            .Select(failure => failure.ErrorMessage)
            .ToArray();

        if (propertyNames.Any())
        {
            LogValidationErrors(request, propertyNames, errorMessages);
            return CreateValidationResult<TResponse>(propertyNames, errorMessages);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(string[] propertyNames, string[] errorMessages)
        where TResult : Result
    {
        if (typeof(TResult).IsAssignableFrom(typeof(Result)))
            return (ValidationResult.WithErrors(propertyNames, errorMessages) as TResult)!;

        object validationResult = typeof(ValidationResult<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(ValidationResult<TResult>.WithErrors))!
            .Invoke(null, new object[] { null!, propertyNames, errorMessages })!;

        return (TResult)validationResult;
    }

    private void LogValidationErrors(TRequest request, string[] propertyNames, string[] errorMessages)
    {
        _logger.LogWarning("Validation failed for request {RequestType}. Validation errors: {Errors}",
                        typeof(TRequest).Name,
                        FormatValidationErrors(propertyNames, errorMessages));
    }

    private static string FormatValidationErrors(string[] propertyNames, string[] errorMessages)
    {
        var formattedErrors = propertyNames
            .Zip(errorMessages, (property, error) => $"{property}: {error}")
            .ToList();

        return string.Join("; ", formattedErrors);
    }
}