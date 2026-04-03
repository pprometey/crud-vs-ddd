using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Repositories;

/// <summary>
/// Repository с маппингом Domain ↔ DbModel для Schedule aggregate
/// </summary>
public sealed class EfScheduleRepository(AppDbContext db) : IScheduleRepository
{
    public ScheduleAgg? FindById(ScheduleId id)
    {
        var dbModel = db.Schedules
            .Include(s => s.Slots)
            .FirstOrDefault(s => s.Id == id.Value);

        return dbModel == null ? null : ScheduleMapper.ToDomain(dbModel);
    }

    public ScheduleAgg? FindByDoctor(UserId doctorId)
    {
        var dbModel = db.Schedules
            .Include(s => s.Slots)
            .FirstOrDefault(s => s.DoctorId == doctorId.Value);

        return dbModel == null ? null : ScheduleMapper.ToDomain(dbModel);
    }

    public TimeSlot? FindSlotById(TimeSlotId slotId)
    {
        var slotDb = db.TimeSlots.FirstOrDefault(s => s.Id == slotId.Value);

        if (slotDb == null) return null;

        return new TimeSlot(
            new TimeSlotId(slotDb.Id),
            slotDb.Start,
            TimeSpan.FromTicks(slotDb.DurationTicks),
            new Money(slotDb.PriceAmount),
            new UserId(slotDb.DoctorId));
    }

    public void Save(ScheduleAgg schedule)
    {
        var dbModel = ScheduleMapper.ToDbModel(schedule);

        var existing = db.Schedules
            .Include(s => s.Slots)
            .FirstOrDefault(s => s.Id == schedule.Id.Value);

        if (existing == null)
        {
            db.Schedules.Add(dbModel);
        }
        else
        {
            existing.DoctorId = dbModel.DoctorId;
            // Version is NOT copied - EF will handle concurrency automatically

            db.Entry(existing).State = EntityState.Modified; // Ensure Version increments

            db.TimeSlots.RemoveRange(existing.Slots);
            foreach (var slot in dbModel.Slots)
            {
                slot.ScheduleId = existing.Id;
                db.TimeSlots.Add(slot);
            }
        }
    }
}
