using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Users.Events;
using Mediator;

namespace DoctorBooking.DDD.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserHandler : Core.Common.Application.CQRS.ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public RegisterUserHandler(IUserRepository userRepo, IUnitOfWork uow, IPublisher publisher)
    {
        _userRepo = userRepo;
        _uow = uow;
        _publisher = publisher;
    }

    public async ValueTask<Result<Guid>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        // Business rule: email must be unique
        var email = new Email(command.Email);
        var existing = _userRepo.FindByEmail(email);
        if (existing is not null)
        {
            return Result<Guid>.Failure(new ValidationError(
                nameof(command.Email),
                "user.email_already_exists",
                "Email is already registered"));
        }

        // Create aggregate - domain validates Email and PersonName
        var role = Enum.Parse<UserRole>(command.Role);
        var user = new UserAgg(
            UserId.New(),
            email,
            new PersonName(command.FirstName, command.LastName),
            role);

        _userRepo.Save(user);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new UserRegistered(user.Id, email, [role]),
            cancellationToken);

        return Result<Guid>.Success(user.Id.Value);
    }
}
