using DoctorBooking.DDD.Application.Users.Errors;
using DoctorBooking.DDD.Domain.Users;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.User.EmailRequired)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.EmailRequired))
            .EmailAddress()
            .WithErrorCode(AppErrorCodes.User.EmailInvalidFormat)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.EmailInvalidFormat));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.User.FirstNameRequired)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.FirstNameRequired));

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.User.LastNameRequired)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.LastNameRequired));

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.User.RoleRequired)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.RoleRequired))
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithErrorCode(AppErrorCodes.User.RoleInvalid)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.RoleInvalid));
    }
}
