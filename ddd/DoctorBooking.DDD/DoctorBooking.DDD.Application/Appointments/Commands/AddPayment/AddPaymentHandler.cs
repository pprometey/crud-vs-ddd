using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Application.Appointments.Errors;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
using Mediator;

namespace DoctorBooking.DDD.Application.Appointments.Commands.AddPayment;

public sealed class AddPaymentHandler : Core.Common.Application.CQRS.ICommandHandler<AddPaymentCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public AddPaymentHandler(IAppointmentRepository appointmentRepo, IUnitOfWork uow, IPublisher publisher)
    {
        _appointmentRepo = appointmentRepo;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result<Guid>> Handle(
        AddPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var appointmentId = new AppointmentId(command.AppointmentId);
        var appointment = _appointmentRepo.FindById(appointmentId);

        if (appointment is null)
        {
            return Result<Guid>.Failure(new ValidationError(
                nameof(command.AppointmentId),
                AppErrorCodes.Appointment.NotFound,
                AppointmentMessages.Msg(AppErrorCodes.Appointment.NotFound, command.AppointmentId)));
        }

        var statusBefore = appointment.Status;

        // Domain validates payment rules (amount, status, total <= price)
        var payment = appointment.AddPayment(PaymentId.New(), new Money(command.Amount), command.PaidAt);

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new PaymentAdded(appointmentId, payment.Id, payment.Amount, payment.PaidAt),
            cancellationToken);

        // If payment caused confirmation (fully paid)
        if (statusBefore != AppointmentStatus.Confirmed && appointment.Status == AppointmentStatus.Confirmed)
        {
            await _publisher.Publish(
                new AppointmentConfirmed(appointmentId, appointment.PatientId, appointment.DoctorId),
                cancellationToken);
        }

        return Result<Guid>.Success(payment.Id.Value);
    }
}
