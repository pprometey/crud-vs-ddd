using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Users.Dtos;
using DoctorBooking.DDD.Application.Users.Queries;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.QueryRepositories;

/// <summary>
/// Query repository для User - работает напрямую с DbModels и проецирует на DTO
/// </summary>
public sealed class EfUserQueryRepository(AppDbContext db) : IUserQueryRepository
{
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ProjectUsers()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await ProjectUsers()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<PagedResult<UserDto>> GetAllPagedAsync(
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var cursor = DecodeCursor(request);

        var query = ProjectUsers();

        if (cursor is not null)
            query = query.ApplyCursor(cursor);
        else
            query = ApplyDefaultSort(query, request);

        var pageSize = request.PageSize;
        var items = await query.Take(pageSize + 1).ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            nextCursor = BuildNextCursor(last, request);
        }

        return new PagedResult<UserDto>(items, nextCursor, hasMore);
    }

    private IQueryable<UserDto> ProjectUsers()
    {
        return db.Users.AsNoTracking()
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Roles.Select(r => ((DoctorBooking.DDD.Domain.Users.UserRole)r.Role).ToString()).ToList(),
                EF.Property<DateTime>(u, "CreatedAt")));
    }

    private static ISortableCursor? DecodeCursor(PageRequest request)
    {
        if (string.IsNullOrEmpty(request.Cursor))
            return null;

        return request.SortBy.ToLowerInvariant() switch
        {
            "email" => UserEmailCursor.Decode(request.Cursor),
            "name" or "lastname" or "last_name" => UserNameCursor.Decode(request.Cursor),
            _ => UserCreatedAtCursor.Decode(request.Cursor)
        };
    }

    private static IQueryable<UserDto> ApplyDefaultSort(IQueryable<UserDto> query, PageRequest request)
    {
        return request.SortBy.ToLowerInvariant() switch
        {
            "email" => request.Direction == SortDirection.Asc
                ? query.OrderBy(u => u.Email).ThenBy(u => u.Id)
                : query.OrderByDescending(u => u.Email).ThenByDescending(u => u.Id),
            "name" or "lastname" or "last_name" => request.Direction == SortDirection.Asc
                ? query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ThenBy(u => u.Id)
                : query.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName).ThenByDescending(u => u.Id),
            _ => request.Direction == SortDirection.Asc
                ? query.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id)
                : query.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id)
        };
    }

    private static string BuildNextCursor(UserDto last, PageRequest request)
    {
        return request.SortBy.ToLowerInvariant() switch
        {
            "email" => new UserEmailCursor(last.Email, last.Id).Encode(),
            "name" or "lastname" or "last_name" => new UserNameCursor(last.LastName, last.FirstName, last.Id).Encode(),
            _ => new UserCreatedAtCursor(last.CreatedAt, last.Id).Encode()
        };
    }
}
