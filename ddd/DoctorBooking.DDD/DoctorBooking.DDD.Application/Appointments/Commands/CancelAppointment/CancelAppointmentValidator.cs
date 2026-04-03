using DoctorBooking.DDD.Application.Appointments.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.IdRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.IdRequired));

        RuleFor(x => x.CancelledById)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.CancelledByRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.CancelledByRequired));
    }
}
