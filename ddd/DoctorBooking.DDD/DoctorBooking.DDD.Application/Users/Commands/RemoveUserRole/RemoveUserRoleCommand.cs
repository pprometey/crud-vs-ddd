using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Users.Commands.RemoveUserRole;

public sealed record RemoveUserRoleCommand(
    Guid UserId,
    string Role
) : ICommand<Result>;
