using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Dtos;

namespace DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentsByDoctor;

public sealed class GetAppointmentsByDoctorHandler
    : IQueryHandler<GetAppointmentsByDoctorQuery, Result<PagedResult<AppointmentDto>>>
{
    private readonly IAppointmentQueryRepository _queryRepo;

    public GetAppointmentsByDoctorHandler(IAppointmentQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<PagedResult<AppointmentDto>>> Handle(
        GetAppointmentsByDoctorQuery query,
        CancellationToken cancellationToken)
    {
        var appointments = await _queryRepo.GetByDoctorPagedAsync(
            query.DoctorId, 
            query.PageRequest, 
            cancellationToken);
        return Result<PagedResult<AppointmentDto>>.Success(appointments);
    }
}
