using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Appointments.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand(
    Guid AppointmentId,
    Guid CancelledById
) : ICommand<Result>;
