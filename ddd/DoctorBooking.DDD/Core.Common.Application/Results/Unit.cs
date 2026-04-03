namespace Core.Common.Application.Results;

/// <summary>
/// Represents a void/valueless return type.
/// </summary>
public readonly struct Unit
{
    public static Unit Value { get; } = default;
}
