using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Schedules.Commands.RemoveSlot;

public sealed record RemoveSlotCommand(
    Guid DoctorId,
    Guid SlotId
) : ICommand<Result>;
