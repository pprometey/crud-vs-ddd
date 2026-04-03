using Core.Common.Domain;
using DoctorBooking.DDD.Application.Schedules.Commands.CreateSchedule;
using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Schedules.Commands;

public class CreateScheduleHandlerTests
{
    private readonly FakeScheduleRepository _scheduleRepo;
    private readonly FakeUserRepository _userRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly CreateScheduleHandler _handler;

    public CreateScheduleHandlerTests()
    {
        _scheduleRepo = new FakeScheduleRepository();
        _userRepo = new FakeUserRepository();
        _uow = new FakeUnitOfWork();

        _handler = new CreateScheduleHandler(_scheduleRepo, _userRepo, _uow);
    }

    private UserAgg CreateDoctor(Guid? doctorId = null)
    {
        var id = new UserId(doctorId ?? Guid.NewGuid());
        var doctor = new UserAgg(
            id,
            new Email($"doctor-{id.Value}@test.com"),
            new PersonName("Dr.", "Smith"),
            UserRole.Doctor);
        _userRepo.Save(doctor);
        return doctor;
    }

    private UserAgg CreatePatient()
    {
        var patient = new UserAgg(
            UserId.New(),
            new Email("patient@test.com"),
            new PersonName("Jane", "Doe"));
        _userRepo.Save(patient);
        return patient;
    }

    [Fact]
    public async Task Handle_ValidDoctor_CreatesScheduleAndReturnsId()
    {
        // Arrange
        var doctor = CreateDoctor();
        var command = new CreateScheduleCommand(DoctorId: doctor.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var schedule = _scheduleRepo.FindById(new ScheduleId(result.Value));
        Assert.NotNull(schedule);
        Assert.Equal(doctor.Id, schedule.DoctorId);
        Assert.Empty(schedule.Slots); // no slots initially
    }

    [Fact]
    public async Task Handle_ValidDoctor_CallsSaveChanges()
    {
        // Arrange
        var doctor = CreateDoctor();
        var command = new CreateScheduleCommand(doctor.Id.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_DoctorNotFound_ThrowsDomainException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new CreateScheduleCommand(nonExistentId);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Verify no changes were saved
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_UserWithoutDoctorRole_ReturnsFailure()
    {
        // Arrange
        var patient = CreatePatient(); // only has Patient role
        var command = new CreateScheduleCommand(patient.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("not_doctor", result.Errors[0].ErrorCode);

        // Verify no schedule was created
        var schedule = _scheduleRepo.FindByDoctor(patient.Id);
        Assert.Null(schedule);

        // Verify no changes were saved
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ScheduleAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var doctor = CreateDoctor();

        // Create initial schedule
        var existingSchedule = new ScheduleAgg(ScheduleId.New(), doctor.Id);
        _scheduleRepo.Save(existingSchedule);

        // Try to create another schedule for the same doctor
        var command = new CreateScheduleCommand(doctor.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("already_exists", result.Errors[0].ErrorCode);

        // Verify SaveChanges was not called
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_UserWithBothPatientAndDoctorRoles_Succeeds()
    {
        // Arrange
        var user = CreatePatient();
        user.AddRole(UserRole.Doctor); // add Doctor role
        _userRepo.Save(user);

        var command = new CreateScheduleCommand(user.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var schedule = _scheduleRepo.FindByDoctor(user.Id);
        Assert.NotNull(schedule);
    }
}
