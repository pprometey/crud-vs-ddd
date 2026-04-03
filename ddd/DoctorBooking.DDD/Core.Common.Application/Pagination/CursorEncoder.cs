using System.Globalization;
using System.Text;

namespace Core.Common.Application.Pagination;

/// <summary>
/// Generic cursor encoder for strongly-typed pagination cursors.
/// Encodes cursor with sort key for validation and type safety.
/// Format: "v2:sortKey#value1|value2|..."
/// </summary>
public static class CursorEncoder
{
    private const string CurrentVersion = "v2";
    
    /// <summary>
    /// Encodes cursor with sort key and multiple values.
    /// </summary>
    public static string Encode(string sortKey, params object[] values)
    {
        var valueParts = values.Select(ConvertToString);
        var raw = $"{CurrentVersion}:{sortKey}#{string.Join("|", valueParts)}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }
    
    /// <summary>
    /// Decodes cursor and returns sort key + values.
    /// Returns null if cursor is invalid or empty.
    /// </summary>
    public static (string SortKey, string[] Values)? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) 
            return null;
        
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            
            if (!decoded.StartsWith($"{CurrentVersion}:"))
                return null;
            
            var withoutVersion = decoded.Substring(CurrentVersion.Length + 1);
            var mainParts = withoutVersion.Split('#', 2);
            
            if (mainParts.Length != 2) 
                return null;
            
            var sortKey = mainParts[0];
            var values = mainParts[1].Split('|');
            
            return (sortKey, values);
        }
        catch
        {
            return null;
        }
    }
    
    private static string ConvertToString(object value) => value switch
    {
        DateTime dt => dt.ToString("O"),
        Guid guid => guid.ToString(),
        string s => s,
        int i => i.ToString(),
        decimal d => d.ToString(CultureInfo.InvariantCulture),
        bool b => b.ToString(),
        _ => value.ToString() ?? ""
    };
}
