using System.Reflection;
using FluentValidation;
using Mediator;
using Core.Common.Application.Results;

namespace Core.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation validators before handler execution.
/// Collects ALL validation errors and returns them as Result.Failure.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(message, cancellationToken);

        var context = new ValidationContext<TRequest>(message);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        if (failures.Count == 0)
            return await next(message, cancellationToken);

        // If TResponse is Result<T> or Result, return Result.Failure with all errors
        var responseType = typeof(TResponse);

        // Check if Result<T>
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var errors = failures.Select(f => new ValidationError(
                f.PropertyName,
                f.ErrorCode,
                f.ErrorMessage)).ToList();

            var failureMethod = responseType.GetMethod(
                nameof(Result<object>.Failure),
                BindingFlags.Public | BindingFlags.Static,
                null,
                [typeof(IReadOnlyList<ValidationError>)],
                null);

            return (TResponse)failureMethod!.Invoke(null, [errors])!;
        }

        // Check if Result (non-generic)
        if (responseType == typeof(Result))
        {
            var errors = failures.Select(f => new ValidationError(
                f.PropertyName,
                f.ErrorCode,
                f.ErrorMessage)).ToList();

            return (TResponse)(object)Result.Failure(errors);
        }

        // Fallback: throw ValidationException for handlers not returning Result
        throw new ValidationException(failures);
    }
}
