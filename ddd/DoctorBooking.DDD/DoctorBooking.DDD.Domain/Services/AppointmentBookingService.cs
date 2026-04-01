using Ardalis.GuardClauses;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Domain.Services;

/// <summary>
/// Domain Service: creates an AppointmentAgg across the ScheduleAgg and AppointmentAgg aggregates.
/// Cross-aggregate logic cannot live inside a single aggregate - it belongs here.
/// </summary>
public sealed class AppointmentBookingService
{
    private readonly IScheduleRepository _scheduleRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IClock _clock;

    public AppointmentBookingService(
        IScheduleRepository scheduleRepo,
        IAppointmentRepository appointmentRepo,
        IUserRepository userRepo,
        IClock clock)
    {
        _scheduleRepo = scheduleRepo;
        _appointmentRepo = appointmentRepo;
        _userRepo = userRepo;
        _clock = clock;
    }

    public AppointmentAgg Book(UserId patientId, TimeSlotId slotId)
    {
        // 1. Verify user exists and has the Patient role
        var user = _userRepo.FindById(patientId)
            ?? throw DomainErrors.User.NotFound(patientId.Value);

        Guard.Against.UserNotPatient(user);

        // 2. Slot must exist
        var slot = _scheduleRepo.FindSlotById(slotId)
            ?? throw new SlotNotFoundException(slotId);

        // 3. Patient cannot book their own schedule slot
        Guard.Against.PatientIsOwnDoctor(patientId, slot.DoctorId);

        // 4. Slot must be in the future
        Guard.Against.SlotInPast(slot.Start, _clock.UtcNow);

        // 5. No CONFIRMED appointment may exist for this slot yet
        var confirmed = _appointmentRepo.FindConfirmedBySlot(slotId);
        if (confirmed is not null)
            throw DomainErrors.Schedule.SlotAlreadyConfirmed();

        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            slotId,
            patientId,
            slot.DoctorId,
            slot.Start,
            slot.Price);

        // Free appointments are confirmed immediately
        if (slot.Price.IsZero())
            appointment.ConfirmFree();

        _appointmentRepo.Save(appointment);
        return appointment;
    }
}
