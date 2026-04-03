using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Dtos;

namespace DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentsByPatient;

public sealed record GetAppointmentsByPatientQuery(Guid PatientId, PageRequest PageRequest) 
    : IQuery<Result<PagedResult<AppointmentDto>>>;
