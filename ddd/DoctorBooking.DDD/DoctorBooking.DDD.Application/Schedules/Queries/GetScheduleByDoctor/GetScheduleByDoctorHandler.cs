using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Schedules.Dtos;
using DoctorBooking.DDD.Application.Schedules.Errors;

namespace DoctorBooking.DDD.Application.Schedules.Queries.GetScheduleByDoctor;

public sealed class GetScheduleByDoctorHandler : IQueryHandler<GetScheduleByDoctorQuery, Result<ScheduleDto>>
{
    private readonly IScheduleQueryRepository _queryRepo;

    public GetScheduleByDoctorHandler(IScheduleQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<ScheduleDto>> Handle(
        GetScheduleByDoctorQuery query,
        CancellationToken cancellationToken)
    {
        var schedule = await _queryRepo.GetByDoctorAsync(query.DoctorId, cancellationToken);

        if (schedule is null)
        {
            return Result<ScheduleDto>.Failure(new ValidationError(
                nameof(query.DoctorId),
                AppErrorCodes.Schedule.NotFound,
                ScheduleMessages.Msg(AppErrorCodes.Schedule.NotFound, query.DoctorId)));
        }

        return Result<ScheduleDto>.Success(schedule);
    }
}
