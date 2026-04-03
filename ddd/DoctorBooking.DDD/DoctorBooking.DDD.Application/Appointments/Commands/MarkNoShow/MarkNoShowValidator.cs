using DoctorBooking.DDD.Application.Appointments.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Appointments.Commands.MarkNoShow;

public sealed class MarkNoShowValidator : AbstractValidator<MarkNoShowCommand>
{
    public MarkNoShowValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.IdRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.IdRequired));
    }
}
