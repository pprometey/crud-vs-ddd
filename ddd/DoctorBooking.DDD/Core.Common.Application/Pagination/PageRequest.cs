namespace Core.Common.Application.Pagination;

/// <summary>
/// Request parameters for cursor-based pagination with dynamic sorting.
/// </summary>
public sealed record PageRequest
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;
    
    public int PageSize { get; init; }
    public string? Cursor { get; init; }
    public string SortBy { get; init; }
    public SortDirection Direction { get; init; }
    
    public PageRequest(
        int pageSize = DefaultPageSize, 
        string? cursor = null,
        string sortBy = "created_at",
        SortDirection direction = SortDirection.Desc)
    {
        PageSize = pageSize is > 0 and <= MaxPageSize ? pageSize : DefaultPageSize;
        Cursor = cursor;
        SortBy = sortBy;
        Direction = direction;
    }
}
