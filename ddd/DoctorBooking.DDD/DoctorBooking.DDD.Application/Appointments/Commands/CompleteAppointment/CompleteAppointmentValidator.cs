using DoctorBooking.DDD.Application.Appointments.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Appointments.Commands.CompleteAppointment;

public sealed class CompleteAppointmentValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.IdRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.IdRequired));
    }
}
