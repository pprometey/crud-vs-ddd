using DoctorBooking.DDD.Application.Schedules.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Schedules.Commands.RemoveSlot;

public sealed class RemoveSlotValidator : AbstractValidator<RemoveSlotCommand>
{
    public RemoveSlotValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Schedule.DoctorIdRequired)
            .WithMessage(_ => ScheduleMessages.Msg(AppErrorCodes.Schedule.DoctorIdRequired));

        RuleFor(x => x.SlotId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Schedule.SlotIdRequired)
            .WithMessage(_ => ScheduleMessages.Msg(AppErrorCodes.Schedule.SlotIdRequired));
    }
}
