using DoctorBooking.DDD.Application.Appointments.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Appointments.Commands.BookAppointment;

public sealed class BookAppointmentValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.PatientIdRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.PatientIdRequired));

        RuleFor(x => x.SlotId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.SlotIdRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.SlotIdRequired));
    }
}
