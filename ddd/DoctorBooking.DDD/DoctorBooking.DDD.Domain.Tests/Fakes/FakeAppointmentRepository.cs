using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Domain.Tests.Fakes;

public class FakeAppointmentRepository : IAppointmentRepository
{
    private readonly Dictionary<AppointmentId, AppointmentAgg> _store = [];

    public AppointmentAgg? FindById(AppointmentId id)
        => _store.TryGetValue(id, out var a) ? a : null;

    public AppointmentAgg? FindConfirmedBySlot(TimeSlotId slotId)
        => _store.Values.FirstOrDefault(a =>
            a.SlotId == slotId && a.Status == AppointmentStatus.Confirmed);

    public IReadOnlyList<AppointmentAgg> FindActiveBySlot(TimeSlotId slotId)
        => _store.Values
            .Where(a => a.SlotId == slotId && !a.Status.IsFinal())
            .ToList();

    public IReadOnlyList<AppointmentAgg> FindByPatient(UserId patientId)
        => _store.Values.Where(a => a.PatientId == patientId).ToList();

    public IReadOnlyList<AppointmentAgg> FindByDoctor(UserId doctorId)
        => _store.Values.Where(a => a.DoctorId == doctorId).ToList();

    public void Save(AppointmentAgg appointment) => _store[appointment.Id] = appointment;
}
