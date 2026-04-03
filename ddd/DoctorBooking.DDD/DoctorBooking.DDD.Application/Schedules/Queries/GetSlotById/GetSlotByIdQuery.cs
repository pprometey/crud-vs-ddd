using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Schedules.Dtos;

namespace DoctorBooking.DDD.Application.Schedules.Queries.GetSlotById;

public sealed record GetSlotByIdQuery(Guid SlotId) : IQuery<Result<TimeSlotDto>>;
