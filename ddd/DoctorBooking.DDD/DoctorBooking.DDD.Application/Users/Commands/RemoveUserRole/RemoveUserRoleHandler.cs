using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Users.Events;
using Mediator;

namespace DoctorBooking.DDD.Application.Users.Commands.RemoveUserRole;

public sealed class RemoveUserRoleHandler : Core.Common.Application.CQRS.ICommandHandler<RemoveUserRoleCommand, Result>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public RemoveUserRoleHandler(IUserRepository userRepo, IUnitOfWork uow, IPublisher publisher)
    {
        _userRepo = userRepo;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(
        RemoveUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(command.UserId);
        var user = _userRepo.FindById(userId)
            ?? throw DomainErrors.User.NotFound(userId.Value);

        var role = Enum.Parse<UserRole>(command.Role);
        user.RemoveRole(role);

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new UserRoleRemoved(userId, role),
            cancellationToken);

        return Result.Success();
    }
}
