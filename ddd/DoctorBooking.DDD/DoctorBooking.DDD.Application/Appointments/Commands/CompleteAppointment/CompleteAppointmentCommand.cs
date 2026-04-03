using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Appointments.Commands.CompleteAppointment;

public sealed record CompleteAppointmentCommand(Guid AppointmentId) : ICommand<Result>;
