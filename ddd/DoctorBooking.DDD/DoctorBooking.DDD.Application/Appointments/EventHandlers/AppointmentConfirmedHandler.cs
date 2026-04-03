using DoctorBooking.DDD.Domain.Appointments.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace DoctorBooking.DDD.Application.Appointments.EventHandlers;

/// <summary>
/// Application-level event handler for AppointmentConfirmed event.
/// Coordinates business logic when an appointment is confirmed (after payment or for free slots).
/// </summary>
public sealed class AppointmentConfirmedHandler : INotificationHandler<AppointmentConfirmed>
{
    private readonly ILogger<AppointmentConfirmedHandler> _logger;

    public AppointmentConfirmedHandler(ILogger<AppointmentConfirmedHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(AppointmentConfirmed notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Appointment {AppointmentId} confirmed for patient {PatientId} with doctor {DoctorId}",
            notification.AppointmentId.Value,
            notification.PatientId.Value,
            notification.DoctorId.Value);

        // Application-level coordination:
        // - Send confirmation email with appointment details
        // - Send SMS reminder to patient
        // - Block the slot in doctor's calendar
        // - Schedule reminder notification 24h before appointment
        // - Update appointment status in read model

        return ValueTask.CompletedTask;
    }
}
