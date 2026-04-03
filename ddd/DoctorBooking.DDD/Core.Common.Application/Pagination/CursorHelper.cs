namespace Core.Common.Application.Pagination;

/// <summary>
/// Helper methods for working with pagination cursor sort keys.
/// </summary>
public static class CursorHelper
{
    /// <summary>
    /// Builds sort key from field name and direction.
    /// Example: "name:asc", "created_at:desc"
    /// </summary>
    public static string BuildSortKey(string fieldName, SortDirection direction) =>
        $"{fieldName}:{(direction == SortDirection.Asc ? "asc" : "desc")}";
    
    /// <summary>
    /// Parses sort key back to field name and direction.
    /// Returns empty field and Asc direction if format is invalid.
    /// </summary>
    public static (string FieldName, SortDirection Direction) ParseSortKey(string? sortKey)
    {
        if (string.IsNullOrEmpty(sortKey))
            return (string.Empty, SortDirection.Asc);
            
        var parts = sortKey.Split(':');
        if (parts.Length != 2)
            return (string.Empty, SortDirection.Asc);
        
        var field = parts[0];
        var directionStr = parts[1];
        
        var direction = directionStr switch
        {
            "asc" => SortDirection.Asc,
            "desc" => SortDirection.Desc,
            _ => (SortDirection?)null
        };
        
        if (direction is null) 
            return (string.Empty, SortDirection.Asc);
            
        return (field, direction.Value);
    }
}
