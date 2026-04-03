using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Dtos;

namespace DoctorBooking.DDD.Application.Appointments.Queries;

/// <summary>
/// Extension methods to apply pagination cursors to Appointment queries.
/// Reduces duplication of WHERE/ORDER BY logic across repositories.
/// </summary>
public static class AppointmentPaginationQueryExtensions
{
    /// <summary>
    /// Applies cursor filtering and ordering to an Appointment query.
    /// Returns query with WHERE clause and ORDER BY applied based on cursor type.
    /// </summary>
    public static IQueryable<AppointmentDto> ApplyCursor(
        this IQueryable<AppointmentDto> query,
        ISortableCursor? cursor)
    {
        return cursor switch
        {
            AppointmentCreatedAtCursor c => query.ApplyCreatedAtCursor(c),
            AppointmentSlotStartCursor c => query.ApplySlotStartCursor(c),
            null => query.ApplyDefaultSort(),
            _ => throw new ArgumentException($"Unknown cursor type: {cursor.GetType().Name}")
        };
    }

    private static IQueryable<AppointmentDto> ApplyCreatedAtCursor(
        this IQueryable<AppointmentDto> query,
        AppointmentCreatedAtCursor cursor)
    {
        if (cursor.Direction == SortDirection.Desc)
        {
            return query
                .Where(a => a.CreatedAt < cursor.CreatedAt ||
                           (a.CreatedAt == cursor.CreatedAt && a.Id.CompareTo(cursor.Id) < 0))
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id);
        }

        return query
            .Where(a => a.CreatedAt > cursor.CreatedAt ||
                       (a.CreatedAt == cursor.CreatedAt && a.Id.CompareTo(cursor.Id) > 0))
            .OrderBy(a => a.CreatedAt)
            .ThenBy(a => a.Id);
    }

    private static IQueryable<AppointmentDto> ApplySlotStartCursor(
        this IQueryable<AppointmentDto> query,
        AppointmentSlotStartCursor cursor)
    {
        if (cursor.Direction == SortDirection.Desc)
        {
            return query
                .Where(a => a.SlotStart < cursor.SlotStart ||
                           (a.SlotStart == cursor.SlotStart && a.Id.CompareTo(cursor.Id) < 0))
                .OrderByDescending(a => a.SlotStart)
                .ThenByDescending(a => a.Id);
        }

        return query
            .Where(a => a.SlotStart > cursor.SlotStart ||
                       (a.SlotStart == cursor.SlotStart && a.Id.CompareTo(cursor.Id) > 0))
            .OrderBy(a => a.SlotStart)
            .ThenBy(a => a.Id);
    }

    private static IQueryable<AppointmentDto> ApplyDefaultSort(this IQueryable<AppointmentDto> query)
    {
        return query
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id);
    }
}
