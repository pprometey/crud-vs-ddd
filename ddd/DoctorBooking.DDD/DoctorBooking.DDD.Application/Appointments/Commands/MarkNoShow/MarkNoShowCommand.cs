using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Appointments.Commands.MarkNoShow;

public sealed record MarkNoShowCommand(Guid AppointmentId) : ICommand<Result>;
