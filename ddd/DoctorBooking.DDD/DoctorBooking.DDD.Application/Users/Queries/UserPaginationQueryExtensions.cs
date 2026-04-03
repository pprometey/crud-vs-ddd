using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Users.Dtos;

namespace DoctorBooking.DDD.Application.Users.Queries;

/// <summary>
/// Extension methods to apply pagination cursors to User queries.
/// Reduces duplication of WHERE/ORDER BY logic across repositories.
/// </summary>
public static class UserPaginationQueryExtensions
{
    /// <summary>
    /// Applies cursor filtering and ordering to a User query.
    /// Returns query with WHERE clause and ORDER BY applied based on cursor type.
    /// </summary>
    public static IQueryable<UserDto> ApplyCursor(
        this IQueryable<UserDto> query,
        ISortableCursor? cursor)
    {
        return cursor switch
        {
            UserCreatedAtCursor c => query.ApplyCreatedAtCursor(c),
            UserNameCursor c => query.ApplyNameCursor(c),
            UserEmailCursor c => query.ApplyEmailCursor(c),
            null => query.ApplyDefaultSort(),
            _ => throw new ArgumentException($"Unknown cursor type: {cursor.GetType().Name}")
        };
    }

    private static IQueryable<UserDto> ApplyCreatedAtCursor(
        this IQueryable<UserDto> query,
        UserCreatedAtCursor cursor)
    {
        if (cursor.Direction == SortDirection.Desc)
        {
            return query
                .Where(u => u.CreatedAt < cursor.CreatedAt ||
                           (u.CreatedAt == cursor.CreatedAt && u.Id.CompareTo(cursor.Id) < 0))
                .OrderByDescending(u => u.CreatedAt)
                .ThenByDescending(u => u.Id);
        }

        return query
            .Where(u => u.CreatedAt > cursor.CreatedAt ||
                       (u.CreatedAt == cursor.CreatedAt && u.Id.CompareTo(cursor.Id) > 0))
            .OrderBy(u => u.CreatedAt)
            .ThenBy(u => u.Id);
    }

    private static IQueryable<UserDto> ApplyNameCursor(
        this IQueryable<UserDto> query,
        UserNameCursor cursor)
    {
        if (cursor.Direction == SortDirection.Desc)
        {
            return query
                .Where(u => u.LastName.CompareTo(cursor.LastName) < 0 ||
                           (u.LastName == cursor.LastName && u.FirstName.CompareTo(cursor.FirstName) < 0) ||
                           (u.LastName == cursor.LastName && u.FirstName == cursor.FirstName && u.Id.CompareTo(cursor.Id) < 0))
                .OrderByDescending(u => u.LastName)
                .ThenByDescending(u => u.FirstName)
                .ThenByDescending(u => u.Id);
        }

        return query
            .Where(u => u.LastName.CompareTo(cursor.LastName) > 0 ||
                       (u.LastName == cursor.LastName && u.FirstName.CompareTo(cursor.FirstName) > 0) ||
                       (u.LastName == cursor.LastName && u.FirstName == cursor.FirstName && u.Id.CompareTo(cursor.Id) > 0))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.Id);
    }

    private static IQueryable<UserDto> ApplyEmailCursor(
        this IQueryable<UserDto> query,
        UserEmailCursor cursor)
    {
        if (cursor.Direction == SortDirection.Desc)
        {
            return query
                .Where(u => u.Email.CompareTo(cursor.Email) < 0 ||
                           (u.Email == cursor.Email && u.Id.CompareTo(cursor.Id) < 0))
                .OrderByDescending(u => u.Email)
                .ThenByDescending(u => u.Id);
        }

        return query
            .Where(u => u.Email.CompareTo(cursor.Email) > 0 ||
                       (u.Email == cursor.Email && u.Id.CompareTo(cursor.Id) > 0))
            .OrderBy(u => u.Email)
            .ThenBy(u => u.Id);
    }

    private static IQueryable<UserDto> ApplyDefaultSort(this IQueryable<UserDto> query)
    {
        return query
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id);
    }
}
