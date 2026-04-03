using DoctorBooking.DDD.Application.Schedules.Dtos;
using DoctorBooking.DDD.Application.Schedules.Queries;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Persistence.QueryRepositories;

public sealed class EfScheduleQueryRepository(AppDbContext db) : IScheduleQueryRepository
{
    public async Task<ScheduleDto?> GetByDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await db.Schedules.AsNoTracking()
            .Where(s => s.DoctorId == new Domain.Users.UserId(doctorId))
            .Select(s => new ScheduleDto(
                s.Id.Value,
                s.DoctorId.Value,
                s.Slots.Select(slot => new TimeSlotDto(
                    slot.Id.Value,
                    slot.Start,
                    slot.Duration,
                    slot.Start + slot.Duration,
                    slot.Price.Amount,
                    slot.DoctorId.Value
                )).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TimeSlotDto?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        return await db.TimeSlots.AsNoTracking()
            .Where(s => s.Id == new TimeSlotId(slotId))
            .Select(s => new TimeSlotDto(
                s.Id.Value,
                s.Start,
                s.Duration,
                s.Start + s.Duration,
                s.Price.Amount,
                s.DoctorId.Value))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeSlotDto>> GetAvailableSlotsAsync(
        Guid doctorId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await db.TimeSlots.AsNoTracking()
            .Where(s => s.DoctorId == new Domain.Users.UserId(doctorId) &&
                        s.Start >= fromDate &&
                        s.Start <= toDate)
            .Select(s => new TimeSlotDto(
                s.Id.Value,
                s.Start,
                s.Duration,
                s.Start + s.Duration,
                s.Price.Amount,
                s.DoctorId.Value))
            .ToListAsync(cancellationToken);
    }
}
