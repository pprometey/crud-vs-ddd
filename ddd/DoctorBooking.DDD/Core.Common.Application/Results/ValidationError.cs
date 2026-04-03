namespace Core.Common.Application.Results;

public readonly record struct ValidationError(
    string Field,
    string ErrorCode,
    string Message);
