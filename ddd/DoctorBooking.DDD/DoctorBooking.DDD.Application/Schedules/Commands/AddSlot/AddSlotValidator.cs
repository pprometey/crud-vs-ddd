using DoctorBooking.DDD.Application.Schedules.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Schedules.Commands.AddSlot;

public sealed class AddSlotValidator : AbstractValidator<AddSlotCommand>
{
    public AddSlotValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Schedule.DoctorIdRequired)
            .WithMessage(_ => ScheduleMessages.Msg(AppErrorCodes.Schedule.DoctorIdRequired));

        RuleFor(x => x.Start)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Schedule.StartRequired)
            .WithMessage(_ => ScheduleMessages.Msg(AppErrorCodes.Schedule.StartRequired));

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero)
            .WithErrorCode(AppErrorCodes.Schedule.DurationPositive)
            .WithMessage(_ => ScheduleMessages.Msg(AppErrorCodes.Schedule.DurationPositive));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(AppErrorCodes.Schedule.PriceNonNegative)
            .WithMessage(_ => ScheduleMessages.Msg(AppErrorCodes.Schedule.PriceNonNegative));
    }
}
