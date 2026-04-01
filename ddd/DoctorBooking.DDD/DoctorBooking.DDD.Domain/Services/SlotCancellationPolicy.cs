using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Schedules;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Services;

/// <summary>
/// Domain Service: guards slot removal when active appointments exist.
/// Called by the Application Layer before invoking Schedule.RemoveSlot().
/// </summary>
public sealed class SlotCancellationPolicy
{
    private readonly IAppointmentRepository _appointmentRepo;

    public SlotCancellationPolicy(IAppointmentRepository appointmentRepo)
    {
        _appointmentRepo = appointmentRepo;
    }

    public void AssertCanRemove(TimeSlotId slotId)
    {
        var active = _appointmentRepo.FindActiveBySlot(slotId);
        Guard.Against.ActiveAppointmentsExist(slotId, active.Count);
    }
}
