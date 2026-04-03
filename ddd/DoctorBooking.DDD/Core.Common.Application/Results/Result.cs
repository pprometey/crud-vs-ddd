namespace Core.Common.Application.Results;

/// <summary>
/// Result for operations that return a value. Contains either a value on success or validation errors on failure.
/// </summary>
public sealed class Result<T>
{
    public T? Value { get; }
    public IReadOnlyList<ValidationError> Errors { get; }
    public bool IsSuccess => Errors.Count == 0;
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        Value = value;
        Errors = Array.Empty<ValidationError>();
    }

    private Result(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(params ValidationError[] errors) => new(errors);
    public static Result<T> Failure(IReadOnlyList<ValidationError> errors) => new(errors);

    /// <summary>
    /// For single domain error (from middleware fallback if domain exception leaked through).
    /// </summary>
    public static Result<T> DomainFailure(string errorCode, string message) =>
        new([new ValidationError("", errorCode, message)]);
}

/// <summary>
/// Result for operations that don't return a value (commands with side effects only).
/// </summary>
public sealed class Result
{
    public IReadOnlyList<ValidationError> Errors { get; }
    public bool IsSuccess => Errors.Count == 0;
    public bool IsFailure => !IsSuccess;

    private Result()
    {
        Errors = Array.Empty<ValidationError>();
    }

    private Result(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
    }

    public static Result Success() => new();
    public static Result Failure(params ValidationError[] errors) => new(errors);
    public static Result Failure(IReadOnlyList<ValidationError> errors) => new(errors);

    public static Result DomainFailure(string errorCode, string message) =>
        new([new ValidationError("", errorCode, message)]);
}
