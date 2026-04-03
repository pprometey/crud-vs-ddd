using Ardalis.GuardClauses;
using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.Persistence;
using Core.Common.Application.CQRS;
using Core.Common.Domain;
using DoctorBooking.DDD.Application.Schedules.Errors;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Schedules.Events;
using DoctorBooking.DDD.Domain.Users;
using Mediator;

namespace DoctorBooking.DDD.Application.Schedules.Commands.AddSlot;

public sealed class AddSlotHandler : Core.Common.Application.CQRS.ICommandHandler<AddSlotCommand, Result<Guid>>
{
    private readonly IScheduleRepository _scheduleRepo;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IPublisher _publisher;

    public AddSlotHandler(IScheduleRepository scheduleRepo, IUnitOfWork uow, IClock clock, IPublisher publisher)
    {
        _scheduleRepo = scheduleRepo;
        _uow = uow;
        _clock = clock;
        _publisher = publisher;
    }

    public async ValueTask<Result<Guid>> Handle(
        AddSlotCommand command,
        CancellationToken cancellationToken)
    {
        var doctorId = new UserId(command.DoctorId);
        var schedule = _scheduleRepo.FindByDoctor(doctorId);

        if (schedule is null)
        {
            return Result<Guid>.Failure(new ValidationError(
                nameof(command.DoctorId),
                AppErrorCodes.Schedule.NotFound,
                ScheduleMessages.Msg(AppErrorCodes.Schedule.NotFound, command.DoctorId)));
        }

        var now = _clock.UtcNow;

        // Contextual check: slot must not be in the past
        Guard.Against.SlotInPast(command.Start, now);

        // Domain validates invariants (no overlap)
        var slot = schedule.AddSlot(
            TimeSlotId.New(),
            command.Start,
            command.Duration,
            new Money(command.Price),
            now);

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new ScheduleSlotAdded(schedule.Id, doctorId, slot.Id, command.Start, command.Duration, command.Price),
            cancellationToken);

        return Result<Guid>.Success(slot.Id.Value);
    }
}
