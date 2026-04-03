using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Dtos;

namespace DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentsByDoctor;

public sealed record GetAppointmentsByDoctorQuery(Guid DoctorId, PageRequest PageRequest) 
    : IQuery<Result<PagedResult<AppointmentDto>>>;
