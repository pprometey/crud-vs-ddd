using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Repositories;

/// <summary>
/// Repository с маппингом Domain ↔ DbModel для Appointment aggregate
/// </summary>
public sealed class EfAppointmentRepository(AppDbContext db) : IAppointmentRepository
{
    public AppointmentAgg? FindById(AppointmentId id)
    {
        var dbModel = db.Appointments
            .Include(a => a.Payments)
            .FirstOrDefault(a => a.Id == id.Value);

        return dbModel == null ? null : AppointmentMapper.ToDomain(dbModel);
    }

    public AppointmentAgg? FindConfirmedBySlot(TimeSlotId slotId)
    {
        var dbModel = db.Appointments
            .Include(a => a.Payments)
            .FirstOrDefault(a => a.SlotId == slotId.Value && a.Status == (int)AppointmentStatus.Confirmed);

        return dbModel == null ? null : AppointmentMapper.ToDomain(dbModel);
    }

    public IReadOnlyList<AppointmentAgg> FindActiveBySlot(TimeSlotId slotId)
    {
        var dbModels = db.Appointments
            .Include(a => a.Payments)
            .Where(a => a.SlotId == slotId.Value &&
                        (a.Status == (int)AppointmentStatus.Planned || a.Status == (int)AppointmentStatus.Confirmed))
            .ToList();

        return dbModels.Select(AppointmentMapper.ToDomain).ToList();
    }

    public void Save(AppointmentAgg appointment)
    {
        var dbModel = AppointmentMapper.ToDbModel(appointment);

        var existing = db.Appointments
            .Include(a => a.Payments)
            .FirstOrDefault(a => a.Id == appointment.Id.Value);

        if (existing == null)
        {
            db.Appointments.Add(dbModel);
        }
        else
        {
            existing.SlotId = dbModel.SlotId;
            existing.PatientId = dbModel.PatientId;
            existing.DoctorId = dbModel.DoctorId;
            existing.SlotStart = dbModel.SlotStart;
            existing.SlotPriceAmount = dbModel.SlotPriceAmount;
            existing.Status = dbModel.Status;
    

            db.Entry(existing).State = EntityState.Modified; // Ensure Version increments

            db.Payments.RemoveRange(existing.Payments);
            foreach (var payment in dbModel.Payments)
            {
                payment.AppointmentId = existing.Id;
                db.Payments.Add(payment);
            }
        }
    }
}
