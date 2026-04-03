namespace Core.Common.Application.Pagination;

/// <summary>
/// Interface for all sortable pagination cursors.
/// </summary>
public interface ISortableCursor
{
    string FieldName { get; }
    SortDirection Direction { get; }
    string Encode();
}
