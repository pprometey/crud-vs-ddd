using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Appointments.Commands.BookAppointment;

public sealed record BookAppointmentCommand(
    Guid PatientId,
    Guid SlotId
) : ICommand<Result<Guid>>;
