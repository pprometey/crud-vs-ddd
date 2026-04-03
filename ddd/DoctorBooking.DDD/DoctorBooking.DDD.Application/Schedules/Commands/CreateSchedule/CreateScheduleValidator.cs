using DoctorBooking.DDD.Application.Schedules.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Schedules.Commands.CreateSchedule;

public sealed class CreateScheduleValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Schedule.DoctorIdRequired)
            .WithMessage(_ => ScheduleMessages.Msg(AppErrorCodes.Schedule.DoctorIdRequired));
    }
}
