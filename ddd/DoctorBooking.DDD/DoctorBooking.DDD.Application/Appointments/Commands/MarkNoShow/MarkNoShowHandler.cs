using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Application.Appointments.Errors;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
using Mediator;

namespace DoctorBooking.DDD.Application.Appointments.Commands.MarkNoShow;

public sealed class MarkNoShowHandler : Core.Common.Application.CQRS.ICommandHandler<MarkNoShowCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public MarkNoShowHandler(IAppointmentRepository appointmentRepo, IUnitOfWork uow, IPublisher publisher)
    {
        _appointmentRepo = appointmentRepo;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(
        MarkNoShowCommand command,
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

        // Domain validates no-show rules (must be Confirmed)
        appointment.MarkNoShow();

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new AppointmentNoShow(appointmentId, appointment.PatientId),
            cancellationToken);

        return Result.Success();
    }
}
