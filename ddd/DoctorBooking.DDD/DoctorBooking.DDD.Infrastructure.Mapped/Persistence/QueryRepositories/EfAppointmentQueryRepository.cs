using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Dtos;
using DoctorBooking.DDD.Application.Appointments.Queries;
using DoctorBooking.DDD.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.QueryRepositories;

/// <summary>
/// Query repository для Appointment - работает напрямую с DbModels и проецирует на DTO
/// </summary>
public sealed class EfAppointmentQueryRepository(AppDbContext db) : IAppointmentQueryRepository
{
    public async Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ProjectAppointments()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<PagedResult<AppointmentDto>> GetByPatientPagedAsync(
        Guid patientId,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = ProjectAppointments().Where(a => a.PatientId == patientId);
        return await ExecutePagedAsync(query, request, cancellationToken);
    }

    public async Task<PagedResult<AppointmentDto>> GetByDoctorPagedAsync(
        Guid doctorId,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = ProjectAppointments().Where(a => a.DoctorId == doctorId);
        return await ExecutePagedAsync(query, request, cancellationToken);
    }

    private IQueryable<AppointmentDto> ProjectAppointments()
    {
        return db.Appointments.AsNoTracking()
            .Select(a => new AppointmentDto(
                a.Id,
                a.SlotId,
                a.PatientId,
                a.DoctorId,
                a.SlotStart,
                a.SlotPriceAmount,
                ((AppointmentStatus)a.Status).ToString(),
                a.Payments.Sum(p => p.Amount),
                a.SlotPriceAmount - a.Payments.Sum(p => p.Amount),
                a.Payments.Select(p => new PaymentDto(
                    p.Id,
                    p.Amount,
                    p.PaidAt,
                    ((PaymentStatus)p.Status).ToString()
                )).ToList(),
                EF.Property<DateTime>(a, "CreatedAt")));
    }

    private static async Task<PagedResult<AppointmentDto>> ExecutePagedAsync(
        IQueryable<AppointmentDto> query,
        PageRequest request,
        CancellationToken cancellationToken)
    {
        var cursor = DecodeCursor(request);

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

        return new PagedResult<AppointmentDto>(items, nextCursor, hasMore);
    }

    private static ISortableCursor? DecodeCursor(PageRequest request)
    {
        if (string.IsNullOrEmpty(request.Cursor))
            return null;

        return request.SortBy.ToLowerInvariant() switch
        {
            "slot_start" => AppointmentSlotStartCursor.Decode(request.Cursor),
            _ => AppointmentCreatedAtCursor.Decode(request.Cursor)
        };
    }

    private static IQueryable<AppointmentDto> ApplyDefaultSort(
        IQueryable<AppointmentDto> query,
        PageRequest request)
    {
        return request.SortBy.ToLowerInvariant() switch
        {
            "slot_start" => request.Direction == SortDirection.Asc
                ? query.OrderBy(a => a.SlotStart).ThenBy(a => a.Id)
                : query.OrderByDescending(a => a.SlotStart).ThenByDescending(a => a.Id),
            _ => request.Direction == SortDirection.Asc
                ? query.OrderBy(a => a.CreatedAt).ThenBy(a => a.Id)
                : query.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id)
        };
    }

    private static string BuildNextCursor(AppointmentDto last, PageRequest request)
    {
        return request.SortBy.ToLowerInvariant() switch
        {
            "slot_start" => new AppointmentSlotStartCursor(last.SlotStart, last.Id).Encode(),
            _ => new AppointmentCreatedAtCursor(last.CreatedAt, last.Id).Encode()
        };
    }
}
