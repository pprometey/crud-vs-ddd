using DoctorBooking.DDD.Application.Schedules.Dtos;
using DoctorBooking.DDD.Application.Schedules.Queries;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.QueryRepositories;

/// <summary>
/// Query repository для Schedule - работает напрямую с DbModels и проецирует на DTO
/// </summary>
public sealed class EfScheduleQueryRepository(AppDbContext db) : IScheduleQueryRepository
{
    public async Task<ScheduleDto?> GetByDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await db.Schedules.AsNoTracking()
            .Where(s => s.DoctorId == doctorId)
            .Select(s => new ScheduleDto(
                s.Id,
                s.DoctorId,
                s.Slots.Select(slot => new TimeSlotDto(
                    slot.Id,
                    slot.Start,
                    TimeSpan.FromTicks(slot.DurationTicks),
                    slot.Start + TimeSpan.FromTicks(slot.DurationTicks),
                    slot.PriceAmount,
                    slot.DoctorId
                )).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TimeSlotDto?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        return await db.TimeSlots.AsNoTracking()
            .Where(s => s.Id == slotId)
            .Select(s => new TimeSlotDto(
                s.Id,
                s.Start,
                TimeSpan.FromTicks(s.DurationTicks),
                s.Start + TimeSpan.FromTicks(s.DurationTicks),
                s.PriceAmount,
                s.DoctorId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeSlotDto>> GetAvailableSlotsAsync(
        Guid doctorId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await db.TimeSlots.AsNoTracking()
            .Where(s => s.DoctorId == doctorId &&
                        s.Start >= fromDate &&
                        s.Start <= toDate)
            .Select(s => new TimeSlotDto(
                s.Id,
                s.Start,
                TimeSpan.FromTicks(s.DurationTicks),
                s.Start + TimeSpan.FromTicks(s.DurationTicks),
                s.PriceAmount,
                s.DoctorId))
            .ToListAsync(cancellationToken);
    }
}
