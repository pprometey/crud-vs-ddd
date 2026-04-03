using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Repositories;

public sealed class EfAppointmentRepository(AppDbContext db) : IAppointmentRepository
{
    public AppointmentAgg? FindById(AppointmentId id)
        => db.Appointments.Include(a => a.Payments).FirstOrDefault(a => a.Id == id);

    public AppointmentAgg? FindConfirmedBySlot(TimeSlotId slotId)
        => db.Appointments.Include(a => a.Payments)
            .FirstOrDefault(a => a.SlotId == slotId && a.Status == AppointmentStatus.Confirmed);

    public IReadOnlyList<AppointmentAgg> FindActiveBySlot(TimeSlotId slotId)
        => db.Appointments.Include(a => a.Payments)
            .Where(a => a.SlotId == slotId &&
                        (a.Status == AppointmentStatus.Planned || a.Status == AppointmentStatus.Confirmed))
            .ToList();

    public void Save(AppointmentAgg appointment)
    {
        if (db.Entry(appointment).State == EntityState.Detached)
            db.Appointments.Add(appointment);
        else
            db.Entry(appointment).State = EntityState.Modified; // Ensure Version increments
    }
}
