using DoctorBooking.DDD.Domain.Appointments.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace DoctorBooking.DDD.Application.Appointments.EventHandlers;

/// <summary>
/// Application-level event handler for AppointmentCreated event.
/// Coordinates business logic when a new appointment is created.
/// </summary>
public sealed class AppointmentCreatedHandler : INotificationHandler<AppointmentCreated>
{
    private readonly ILogger<AppointmentCreatedHandler> _logger;

    public AppointmentCreatedHandler(ILogger<AppointmentCreatedHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(AppointmentCreated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Appointment {AppointmentId} created for patient {PatientId} with doctor {DoctorId} at slot {SlotId}",
            notification.AppointmentId.Value,
            notification.PatientId.Value,
            notification.DoctorId.Value,
            notification.SlotId.Value);

        // Application-level coordination:
        // - Send confirmation email to patient
        // - Send notification to doctor
        // - Update read model / projection for appointments list
        // - Trigger analytics event
        // - Add to doctor's calendar (if integration exists)

        return ValueTask.CompletedTask;
    }
}
