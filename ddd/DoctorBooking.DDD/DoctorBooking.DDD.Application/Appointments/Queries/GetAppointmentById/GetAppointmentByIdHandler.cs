using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Appointments.Dtos;
using DoctorBooking.DDD.Application.Appointments.Errors;

namespace DoctorBooking.DDD.Application.Appointments.Queries.GetAppointmentById;

public sealed class GetAppointmentByIdHandler : IQueryHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    private readonly IAppointmentQueryRepository _queryRepo;

    public GetAppointmentByIdHandler(IAppointmentQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<AppointmentDto>> Handle(
        GetAppointmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var appointment = await _queryRepo.GetByIdAsync(query.AppointmentId, cancellationToken);

        if (appointment is null)
        {
            return Result<AppointmentDto>.Failure(new ValidationError(
                nameof(query.AppointmentId),
                AppErrorCodes.Appointment.NotFound,
                AppointmentMessages.Msg(AppErrorCodes.Appointment.NotFound, query.AppointmentId)));
        }

        return Result<AppointmentDto>.Success(appointment);
    }
}
