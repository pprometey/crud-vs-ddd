namespace Core.Common.Application.Pagination;

/// <summary>
/// Result with pagination metadata for cursor-based pagination.
/// </summary>
public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
    public int TotalReturned { get; init; }
    
    public PagedResult(
        IReadOnlyList<T> items, 
        string? nextCursor, 
        bool hasMore)
    {
        Items = items;
        NextCursor = nextCursor;
        HasMore = hasMore;
        TotalReturned = items.Count;
    }
    
    public static PagedResult<T> Empty() => new(
        Array.Empty<T>(), 
        null, 
        false);
}
