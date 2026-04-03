using System.Globalization;
using Core.Common.Application.Pagination;

namespace DoctorBooking.DDD.Application.Users.Queries;

/// <summary>
/// Cursor for User pagination sorted by CreatedAt + Id.
/// Supports both ASC and DESC directions.
/// </summary>
public sealed record UserCreatedAtCursor(
    DateTime CreatedAt,
    Guid Id) : SortableCursor<UserCreatedAtCursor>
{
    public const string Field = "created_at";
    
    public override string FieldName => Field;
    
    protected override object[] GetValues() => new object[] { CreatedAt, Id };
    
    public static UserCreatedAtCursor? Decode(string? cursor) =>
        DecodeBase(cursor, Field, (values, dir) =>
            new UserCreatedAtCursor(
                DateTime.Parse(values[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), 
                Guid.Parse(values[1])) with { Direction = dir });
}

/// <summary>
/// Cursor for User pagination sorted by Email + Id.
/// Supports both ASC and DESC directions.
/// </summary>
public sealed record UserEmailCursor(
    string Email,
    Guid Id) : SortableCursor<UserEmailCursor>
{
    public const string Field = "email";
    
    public override string FieldName => Field;
    
    protected override object[] GetValues() => new object[] { Email, Id };
    
    public static UserEmailCursor? Decode(string? cursor) =>
        DecodeBase(cursor, Field, (values, dir) =>
            new UserEmailCursor(values[0], Guid.Parse(values[1])) with { Direction = dir });
}

/// <summary>
/// Cursor for User pagination sorted by LastName + FirstName + Id.
/// Supports both ASC and DESC directions.
/// </summary>
public sealed record UserNameCursor(
    string LastName,
    string FirstName,
    Guid Id) : SortableCursor<UserNameCursor>
{
    public const string Field = "name";
    
    public override string FieldName => Field;
    
    protected override object[] GetValues() => new object[] { LastName, FirstName, Id };
    
    public static UserNameCursor? Decode(string? cursor) =>
        DecodeBase(cursor, Field, (values, dir) =>
            new UserNameCursor(values[0], values[1], Guid.Parse(values[2])) with { Direction = dir });
}
