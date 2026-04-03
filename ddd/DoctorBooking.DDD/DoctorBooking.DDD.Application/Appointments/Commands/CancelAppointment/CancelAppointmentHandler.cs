using Ardalis.GuardClauses;
using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using Core.Common.Domain;
using DoctorBooking.DDD.Application.Appointments.Errors;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Users;
using Mediator;

namespace DoctorBooking.DDD.Application.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentHandler : Core.Common.Application.CQRS.ICommandHandler<CancelAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IPublisher _publisher;

    public CancelAppointmentHandler(
        IAppointmentRepository appointmentRepo,
        IUnitOfWork uow,
        IClock clock,
        IPublisher publisher)
    {
        _appointmentRepo = appointmentRepo;
        _uow = uow;
        _clock = clock;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(
        CancelAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var appointmentId = new AppointmentId(command.AppointmentId);
        var appointment = _appointmentRepo.FindById(appointmentId);

        if (appointment is null)
        {
            return Result.Failure(new ValidationError(
                nameof(command.AppointmentId),
                AppErrorCodes.Appointment.NotFound,
                AppointmentMessages.Msg(AppErrorCodes.Appointment.NotFound, command.AppointmentId)));
        }

        var now = _clock.UtcNow;
        var cancelledBy = new UserId(command.CancelledById);

        // Contextual check: appointment must not have already started
        Guard.Against.AppointmentAlreadyStarted(now, appointment.SlotStart);

        // Domain validates cancellation rules (status)
        appointment.Cancel(cancelledBy, now);

        await _uow.SaveChangesAsync(cancellationToken);

        var shouldRefund = appointment.ShouldRefund(now);

        await _publisher.Publish(
            new AppointmentCancelled(appointmentId, cancelledBy, shouldRefund, appointment.Payments),
            cancellationToken);

        return Result.Success();
    }
}
