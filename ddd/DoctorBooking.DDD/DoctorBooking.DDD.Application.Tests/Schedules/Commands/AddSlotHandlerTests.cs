using Core.Common.Domain;
using DoctorBooking.DDD.Application.Schedules.Commands.AddSlot;
using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Schedules.Commands;

public class AddSlotHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureSlotStart = Now.AddDays(3);

    private readonly FakeScheduleRepository _scheduleRepo;
    private readonly FakeClock _clock;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly AddSlotHandler _handler;

    public AddSlotHandlerTests()
    {
        _scheduleRepo = new FakeScheduleRepository();
        _clock = new FakeClock(Now);
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();

        _handler = new AddSlotHandler(_scheduleRepo, _uow, _clock, _publisher);
    }

    private void CreateScheduleForDoctor(UserId doctorId)
    {
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        _scheduleRepo.Save(schedule);
    }

    [Fact]
    public async Task Handle_ValidSlot_AddsSlotAndReturnsId()
    {
        // Arrange
        var doctorId = UserId.New();
        CreateScheduleForDoctor(doctorId);

        var command = new AddSlotCommand(
            DoctorId: doctorId.Value,
            Start: FutureSlotStart,
            Duration: TimeSpan.FromHours(1),
            Price: 100m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var updated = _scheduleRepo.FindByDoctor(doctorId);
        Assert.NotNull(updated);
        Assert.Single(updated.Slots);
        Assert.Equal(FutureSlotStart, updated.Slots[0].Start);
        Assert.Equal(TimeSpan.FromHours(1), updated.Slots[0].Duration);
        Assert.Equal(new Money(100), updated.Slots[0].Price);
    }

    [Fact]
    public async Task Handle_ValidSlot_CallsSaveChanges()
    {
        // Arrange
        var doctorId = UserId.New();
        CreateScheduleForDoctor(doctorId);

        var command = new AddSlotCommand(
            doctorId.Value,
            FutureSlotStart,
            TimeSpan.FromHours(1),
            100m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ScheduleNotFound_ReturnsFailure()
    {
        // Arrange
        var nonExistentDoctorId = Guid.NewGuid();
        var command = new AddSlotCommand(
            nonExistentDoctorId,
            FutureSlotStart,
            TimeSpan.FromHours(1),
            100m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("not_found", result.Errors[0].ErrorCode);

        // Verify no changes were saved
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_SlotInPast_ThrowsDomainException()
    {
        // Arrange
        var doctorId = UserId.New();
        CreateScheduleForDoctor(doctorId);

        var pastSlotStart = Now.AddDays(-1);
        var command = new AddSlotCommand(
            doctorId.Value,
            pastSlotStart,
            TimeSpan.FromHours(1),
            100m);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MultipleSlots_AddsAll()
    {
        // Arrange
        var doctorId = UserId.New();
        CreateScheduleForDoctor(doctorId);

        // Add first slot
        var command1 = new AddSlotCommand(
            doctorId.Value,
            FutureSlotStart,
            TimeSpan.FromHours(1),
            100m);
        await _handler.Handle(command1, CancellationToken.None);

        // Add second slot (different time)
        var command2 = new AddSlotCommand(
            doctorId.Value,
            FutureSlotStart.AddHours(2),
            TimeSpan.FromHours(1),
            150m);

        // Act
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.True(result2.IsSuccess);

        var updated = _scheduleRepo.FindByDoctor(doctorId);
        Assert.NotNull(updated);
        Assert.Equal(2, updated.Slots.Count);
    }

    [Fact]
    public async Task Handle_OverlappingSlot_ThrowsDomainException()
    {
        // Arrange
        var doctorId = UserId.New();
        CreateScheduleForDoctor(doctorId);

        // Add first slot (10:00 - 11:00)
        var command1 = new AddSlotCommand(
            doctorId.Value,
            FutureSlotStart,
            TimeSpan.FromHours(1),
            100m);
        await _handler.Handle(command1, CancellationToken.None);

        // Try to add overlapping slot (10:30 - 11:30)
        var command2 = new AddSlotCommand(
            doctorId.Value,
            FutureSlotStart.AddMinutes(30),
            TimeSpan.FromHours(1),
            100m);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command2, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ZeroPrice_CreatesFreeSlot()
    {
        // Arrange
        var doctorId = UserId.New();
        CreateScheduleForDoctor(doctorId);

        var command = new AddSlotCommand(
            doctorId.Value,
            FutureSlotStart,
            TimeSpan.FromHours(1),
            0m); // Free

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = _scheduleRepo.FindByDoctor(doctorId);
        Assert.Single(updated!.Slots);
        Assert.Equal(Money.Zero, updated.Slots[0].Price);
    }

    [Fact]
    public async Task Handle_NegativePrice_ThrowsDomainException()
    {
        // Arrange
        var doctorId = UserId.New();
        CreateScheduleForDoctor(doctorId);

        var command = new AddSlotCommand(
            doctorId.Value,
            FutureSlotStart,
            TimeSpan.FromHours(1),
            -50m); // Negative

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }
}
