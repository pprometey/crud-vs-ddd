using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Appointments.Commands.AddPayment;

public sealed record AddPaymentCommand(
    Guid AppointmentId,
    decimal Amount,
    DateTime PaidAt
) : ICommand<Result<Guid>>;
