namespace Core.Common.Application.Pagination;

/// <summary>
/// Base class for strongly-typed pagination cursors with dynamic sorting support.
/// Reduces boilerplate code by providing common Encode/Decode logic.
/// </summary>
/// <typeparam name="TSelf">The concrete cursor type (CRTP pattern)</typeparam>
public abstract record SortableCursor<TSelf> : ISortableCursor 
    where TSelf : SortableCursor<TSelf>
{
    /// <summary>
    /// Field name for this cursor (e.g., "name", "created_at", "email").
    /// </summary>
    public abstract string FieldName { get; }
    
    /// <summary>
    /// Sort direction for this cursor.
    /// </summary>
    public SortDirection Direction { get; init; }
    
    /// <summary>
    /// Gets the values to encode in the cursor (in order).
    /// </summary>
    protected abstract object[] GetValues();
    
    /// <summary>
    /// Encodes cursor into a base64 string with sort key.
    /// </summary>
    public string Encode() =>
        CursorEncoder.Encode(
            CursorHelper.BuildSortKey(FieldName, Direction),
            GetValues());
    
    /// <summary>
    /// Base decode method that handles sort key parsing and validation.
    /// </summary>
    protected static TSelf? DecodeBase(
        string? cursor, 
        string expectedField, 
        Func<string[], SortDirection, TSelf> factory)
    {
        var data = CursorEncoder.Decode(cursor);
        if (data is null) 
            return null;
        
        var parsed = CursorHelper.ParseSortKey(data.Value.SortKey);
        if (string.IsNullOrEmpty(parsed.FieldName) || parsed.FieldName != expectedField)
            return null;
        
        try
        {
            return factory(data.Value.Values, parsed.Direction);
        }
        catch
        {
            return null;
        }
    }
}
