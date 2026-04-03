using DoctorBooking.DDD.Application.Users.Errors;
using DoctorBooking.DDD.Domain.Users;
using FluentValidation;

namespace DoctorBooking.DDD.Application.Users.Commands.AddUserRole;

public sealed class AddUserRoleValidator : AbstractValidator<AddUserRoleCommand>
{
    public AddUserRoleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.User.IdRequired)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.IdRequired));

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithErrorCode(AppErrorCodes.User.RoleRequired)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.RoleRequired))
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithErrorCode(AppErrorCodes.User.RoleInvalid)
            .WithMessage(_ => UserMessages.Msg(AppErrorCodes.User.RoleInvalid));
    }
}
