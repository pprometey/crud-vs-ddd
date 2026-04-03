using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Appointments.Dtos;

namespace DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentById;

public sealed record GetAppointmentByIdQuery(Guid AppointmentId) : IQuery<Result<AppointmentDto>>;
