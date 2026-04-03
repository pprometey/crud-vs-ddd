using DoctorBooking.DDD.Application.Appointments.Errors;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Appointments.Commands.AddPayment;

public sealed class AddPaymentValidator : AbstractValidator<AddPaymentCommand>
{
    public AddPaymentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.IdRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.IdRequired));

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithErrorCode(AppErrorCodes.Appointment.AmountPositive)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.AmountPositive));

        RuleFor(x => x.PaidAt)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.Appointment.PaidAtRequired)
            .WithMessage(_ => AppointmentMessages.Msg(AppErrorCodes.Appointment.PaidAtRequired));
    }
}
