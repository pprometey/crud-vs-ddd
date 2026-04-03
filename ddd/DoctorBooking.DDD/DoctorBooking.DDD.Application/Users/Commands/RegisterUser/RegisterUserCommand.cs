using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Role
) : ICommand<Result<Guid>>;
