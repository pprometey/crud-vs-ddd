using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Domain.Appointments;

public interface IAppointmentRepository
{
    AppointmentAgg? FindById(AppointmentId id);

    /// <summary>Returns the single CONFIRMED appointment for a slot, or null.</summary>
    AppointmentAgg? FindConfirmedBySlot(TimeSlotId slotId);

    /// <summary>Returns all non-final (PLANNED or CONFIRMED) appointments for a slot.</summary>
    IReadOnlyList<AppointmentAgg> FindActiveBySlot(TimeSlotId slotId);

    IReadOnlyList<AppointmentAgg> FindByPatient(UserId patientId);
    IReadOnlyList<AppointmentAgg> FindByDoctor(UserId doctorId);

    void Save(AppointmentAgg appointment);
}
