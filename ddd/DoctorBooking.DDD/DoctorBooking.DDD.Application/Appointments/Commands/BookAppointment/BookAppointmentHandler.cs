using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Services;
using DoctorBooking.DDD.Domain.Users;
using Mediator;

namespace DoctorBooking.DDD.Application.Appointments.Commands.BookAppointment;

public sealed class BookAppointmentHandler : Core.Common.Application.CQRS.ICommandHandler<BookAppointmentCommand, Result<Guid>>
{
    private readonly AppointmentBookingService _bookingService;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public BookAppointmentHandler(AppointmentBookingService bookingService, IUnitOfWork uow, IPublisher publisher)
    {
        _bookingService = bookingService;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result<Guid>> Handle(
        BookAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        // Domain service handles all cross-aggregate logic and invariants
        var appointment = _bookingService.Book(
            new UserId(command.PatientId),
            new TimeSlotId(command.SlotId));

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new AppointmentCreated(appointment.Id, appointment.PatientId, appointment.DoctorId, appointment.SlotId),
            cancellationToken);

        if (appointment.Status == AppointmentStatus.Confirmed)
        {
            await _publisher.Publish(
                new AppointmentConfirmed(appointment.Id, appointment.PatientId, appointment.DoctorId),
                cancellationToken);
        }

        return Result<Guid>.Success(appointment.Id.Value);
    }
}
