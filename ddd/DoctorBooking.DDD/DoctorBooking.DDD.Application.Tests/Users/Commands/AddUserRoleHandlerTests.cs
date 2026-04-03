using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Application.Users.Commands.AddUserRole;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Users.Commands;

public class AddUserRoleHandlerTests
{
    private readonly FakeUserRepository _userRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly AddUserRoleHandler _handler;

    public AddUserRoleHandlerTests()
    {
        _userRepo = new FakeUserRepository();
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();
        _handler = new AddUserRoleHandler(_userRepo, _uow, _publisher);
    }

    private UserAgg CreatePatient(Guid? userId = null)
    {
        var id = new UserId(userId ?? Guid.NewGuid());
        var user = new UserAgg(
            id,
            new Email($"user-{id.Value}@test.com"),
            new PersonName("John", "Doe"));
        _userRepo.Save(user);
        return user;
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsRoleToUser()
    {
        // Arrange
        var user = CreatePatient();
        var command = new AddUserRoleCommand(
            UserId: user.Id.Value,
            Role: "Doctor");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = _userRepo.FindById(user.Id);
        Assert.NotNull(updated);
        Assert.True(updated.HasRole(UserRole.Doctor));
        Assert.True(updated.HasRole(UserRole.Patient)); // still has Patient
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChanges()
    {
        // Arrange
        var user = CreatePatient();
        var command = new AddUserRoleCommand(user.Id.Value, "Doctor");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AddAdminRole_Succeeds()
    {
        // Arrange
        var user = CreatePatient();
        var command = new AddUserRoleCommand(user.Id.Value, "Doctor");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = _userRepo.FindById(user.Id);
        Assert.NotNull(updated);
        Assert.True(updated.HasRole(UserRole.Doctor));
    }

    [Fact]
    public async Task Handle_AddExistingRole_IsIdempotent()
    {
        // Arrange
        var user = CreatePatient();

        var command = new AddUserRoleCommand(user.Id.Value, "Patient"); // already has Patient

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = _userRepo.FindById(user.Id);
        Assert.NotNull(updated);
        Assert.Single(updated.Roles); // still just one role
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsDomainException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AddUserRoleCommand(nonExistentId, "Doctor");

        // Act & Assert
        await Assert.ThrowsAsync<Core.Common.Domain.DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Verify no changes were saved
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

}
