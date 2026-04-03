using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Schedules.Commands.AddSlot;

public sealed record AddSlotCommand(
    Guid DoctorId,
    DateTime Start,
    TimeSpan Duration,
    decimal Price
) : ICommand<Result<Guid>>;
