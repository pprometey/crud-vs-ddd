using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;

namespace DoctorBooking.DDD.Domain.Appointments;

public interface IAppointmentRepository
{
    AppointmentAgg? FindById(AppointmentId id);

    /// <summary>Returns the single CONFIRMED appointment for a slot, or null.</summary>
    AppointmentAgg? FindConfirmedBySlot(TimeSlotId slotId);

    /// <summary>Returns all non-final (PLANNED or CONFIRMED) appointments for a slot.</summary>
    IReadOnlyList<AppointmentAgg> FindActiveBySlot(TimeSlotId slotId);

    void Save(AppointmentAgg appointment);
}
