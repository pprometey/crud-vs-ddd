using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Users.Events;
using Mediator;

namespace DoctorBooking.DDD.Application.Users.Commands.AddUserRole;

public sealed class AddUserRoleHandler : Core.Common.Application.CQRS.ICommandHandler<AddUserRoleCommand, Result>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public AddUserRoleHandler(IUserRepository userRepo, IUnitOfWork uow, IPublisher publisher)
    {
        _userRepo = userRepo;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result> Handle(
        AddUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(command.UserId);
        var user = _userRepo.FindById(userId)
            ?? throw DomainErrors.User.NotFound(userId.Value);

        var role = Enum.Parse<UserRole>(command.Role);

        if (user.HasRole(role))
            return Result.Success(); // idempotent

        user.AddRole(role);

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new UserRoleAdded(userId, role),
            cancellationToken);

        return Result.Success();
    }
}
