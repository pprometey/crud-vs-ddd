using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Application.Appointments.Errors;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
using Mediator;

namespace DoctorBooking.DDD.Application.Appointments.Commands.CompleteAppointment;

public sealed class CompleteAppointmentHandler : Core.Common.Application.CQRS.ICommandHandler<CompleteAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public CompleteAppointmentHandler(IAppointmentRepository appointmentRepo, IUnitOfWork uow, IPublisher publisher)
    {
        _appointmentRepo = appointmentRepo;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(
        CompleteAppointmentCommand command,
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

        // Domain validates completion rules (must be Confirmed)
        appointment.Complete();

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new AppointmentCompleted(appointmentId, appointment.DoctorId),
            cancellationToken);

        return Result.Success();
    }
}
