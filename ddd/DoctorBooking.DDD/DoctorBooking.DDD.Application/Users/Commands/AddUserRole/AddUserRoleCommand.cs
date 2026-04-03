using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Users.Commands.AddUserRole;

public sealed record AddUserRoleCommand(
    Guid UserId,
    string Role
) : ICommand<Result>;
