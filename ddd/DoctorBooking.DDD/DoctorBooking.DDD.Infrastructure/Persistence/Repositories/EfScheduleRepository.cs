using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Repositories;

public sealed class EfScheduleRepository(AppDbContext db) : IScheduleRepository
{
    public ScheduleAgg? FindById(ScheduleId id)
        => db.Schedules.Include(s => s.Slots).FirstOrDefault(s => s.Id == id);

    public ScheduleAgg? FindByDoctor(UserId doctorId)
        => db.Schedules.Include(s => s.Slots).FirstOrDefault(s => s.DoctorId == doctorId);

    public TimeSlot? FindSlotById(TimeSlotId slotId)
        => db.TimeSlots.FirstOrDefault(s => s.Id == slotId);

    public void Save(ScheduleAgg schedule)
    {
        if (db.Entry(schedule).State == EntityState.Detached)
            db.Schedules.Add(schedule);
        else
            db.Entry(schedule).State = EntityState.Modified; // Ensure Version increments
    }
}
