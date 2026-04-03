using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Schedules.Dtos;

namespace DoctorBooking.DDD.Application.Schedules.Queries.GetSlotById;

public sealed class GetSlotByIdHandler : IQueryHandler<GetSlotByIdQuery, Result<TimeSlotDto>>
{
    private readonly IScheduleQueryRepository _queryRepo;

    public GetSlotByIdHandler(IScheduleQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<TimeSlotDto>> Handle(
        GetSlotByIdQuery query,
        CancellationToken cancellationToken)
    {
        var slot = await _queryRepo.GetSlotByIdAsync(query.SlotId, cancellationToken);

        if (slot is null)
        {
            return Result<TimeSlotDto>.Failure(new ValidationError(
                nameof(query.SlotId),
                "schedule.slot_not_found",
                $"Slot {query.SlotId} was not found"));
        }

        return Result<TimeSlotDto>.Success(slot);
    }
}
