using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Dtos;

namespace DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentsByPatient;

public sealed class GetAppointmentsByPatientHandler
    : IQueryHandler<GetAppointmentsByPatientQuery, Result<PagedResult<AppointmentDto>>>
{
    private readonly IAppointmentQueryRepository _queryRepo;

    public GetAppointmentsByPatientHandler(IAppointmentQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<PagedResult<AppointmentDto>>> Handle(
        GetAppointmentsByPatientQuery query,
        CancellationToken cancellationToken)
    {
        var appointments = await _queryRepo.GetByPatientPagedAsync(
            query.PatientId, 
            query.PageRequest, 
            cancellationToken);
        return Result<PagedResult<AppointmentDto>>.Success(appointments);
    }
}
