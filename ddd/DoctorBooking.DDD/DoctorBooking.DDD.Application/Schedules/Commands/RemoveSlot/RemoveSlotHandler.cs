using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Application.Schedules.Errors;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Schedules.Events;
using DoctorBooking.DDD.Domain.Services;
using DoctorBooking.DDD.Domain.Users;
using Mediator;

namespace DoctorBooking.DDD.Application.Schedules.Commands.RemoveSlot;

public sealed class RemoveSlotHandler : Core.Common.Application.CQRS.ICommandHandler<RemoveSlotCommand, Result>
{
    private readonly IScheduleRepository _scheduleRepo;
    private readonly SlotCancellationPolicy _cancellationPolicy;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public RemoveSlotHandler(
        IScheduleRepository scheduleRepo,
        SlotCancellationPolicy cancellationPolicy,
        IUnitOfWork uow,
        IPublisher publisher)
    {
        _scheduleRepo = scheduleRepo;
        _cancellationPolicy = cancellationPolicy;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(
        RemoveSlotCommand command,
        CancellationToken cancellationToken)
    {
        var doctorId = new UserId(command.DoctorId);
        var schedule = _scheduleRepo.FindByDoctor(doctorId);

        if (schedule is null)
        {
            return Result.Failure(new ValidationError(
                nameof(command.DoctorId),
                AppErrorCodes.Schedule.NotFound,
                ScheduleMessages.Msg(AppErrorCodes.Schedule.NotFound, command.DoctorId)));
        }

        var slotId = new TimeSlotId(command.SlotId);

        // Domain service checks for active appointments
        _cancellationPolicy.AssertCanRemove(slotId);

        schedule.RemoveSlot(slotId);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new ScheduleSlotRemoved(schedule.Id, slotId),
            cancellationToken);

        return Result.Success();
    }
}
