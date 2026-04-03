using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Application.Users.Commands.RegisterUser;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Users.Commands;

public class RegisterUserHandlerTests
{
    private readonly FakeUserRepository _userRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _userRepo = new FakeUserRepository();
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();
        _handler = new RegisterUserHandler(_userRepo, _uow, _publisher);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndReturnsId()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "new.patient@test.com",
            FirstName: "John",
            LastName: "Doe",
            Role: "Patient");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify user was saved
        var savedUser = _userRepo.FindByEmail(new Email("new.patient@test.com"));
        Assert.NotNull(savedUser);
        Assert.Equal("new.patient@test.com", savedUser.Email.Value);
        Assert.Equal("John", savedUser.Name.FirstName);
        Assert.Equal("Doe", savedUser.Name.LastName);
        Assert.True(savedUser.HasRole(UserRole.Patient));
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChanges()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "john@test.com",
            FirstName: "John",
            LastName: "Doe",
            Role: "Patient");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var existingEmail = "john@test.com";
        var existingUser = new UserAgg(
            UserId.New(),
            new Email(existingEmail),
            new PersonName("Existing", "User"));
        _userRepo.Save(existingUser);

        var command = new RegisterUserCommand(
            Email: existingEmail,
            FirstName: "John",
            LastName: "Doe",
            Role: "Patient");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("email", result.Errors[0].ErrorCode.ToLowerInvariant());

        // Verify SaveChanges was not called
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_DoctorRole_CreatesUserWithCorrectRole()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "doctor@test.com",
            FirstName: "Dr.",
            LastName: "Smith",
            Role: "Doctor");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var savedUser = _userRepo.FindByEmail(new Email("doctor@test.com"));
        Assert.NotNull(savedUser);
        Assert.True(savedUser.HasRole(UserRole.Doctor));
    }

    [Fact]
    public async Task Handle_AdminRole_CreatesUserWithCorrectRole()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "admin@test.com",
            FirstName: "Admin",
            LastName: "User",
            Role: "Doctor");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var savedUser = _userRepo.FindByEmail(new Email("admin@test.com"));
        Assert.NotNull(savedUser);
        Assert.True(savedUser.HasRole(UserRole.Doctor));
    }
}
