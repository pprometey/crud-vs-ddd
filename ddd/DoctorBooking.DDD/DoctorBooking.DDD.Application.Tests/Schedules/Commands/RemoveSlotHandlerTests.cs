using Core.Common.Domain;
using DoctorBooking.DDD.Application.Schedules.Commands.RemoveSlot;
using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Services;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Schedules.Commands;

public class RemoveSlotHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureSlotStart = Now.AddDays(3);

    private readonly FakeScheduleRepository _scheduleRepo;
    private readonly FakeAppointmentRepository _appointmentRepo;
    private readonly SlotCancellationPolicy _cancellationPolicy;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly RemoveSlotHandler _handler;

    public RemoveSlotHandlerTests()
    {
        _scheduleRepo = new FakeScheduleRepository();
        _appointmentRepo = new FakeAppointmentRepository();
        _cancellationPolicy = new SlotCancellationPolicy(_appointmentRepo);
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();

        _handler = new RemoveSlotHandler(_scheduleRepo, _cancellationPolicy, _uow, _publisher);
    }

    private (ScheduleAgg schedule, TimeSlot slot) CreateScheduleWithSlot(UserId doctorId)
    {
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        var slotId = TimeSlotId.New();
        var slot = schedule.AddSlot(
            slotId,
            FutureSlotStart,
            TimeSpan.FromHours(1),
            new Money(100),
            Now);
        _scheduleRepo.Save(schedule);
        return (schedule, slot);
    }

    [Fact]
    public async Task Handle_SlotWithNoAppointments_RemovesSuccessfully()
    {
        // Arrange
        var doctorId = UserId.New();
        var (_, slot) = CreateScheduleWithSlot(doctorId);

        var command = new RemoveSlotCommand(
            DoctorId: doctorId.Value,
            SlotId: slot.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = _scheduleRepo.FindByDoctor(doctorId);
        Assert.NotNull(updated);
        Assert.Empty(updated.Slots);
    }

    [Fact]
    public async Task Handle_ValidRemoval_CallsSaveChanges()
    {
        // Arrange
        var doctorId = UserId.New();
        var (_, slot) = CreateScheduleWithSlot(doctorId);

        var command = new RemoveSlotCommand(doctorId.Value, slot.Id.Value);

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
        var slotId = Guid.NewGuid();

        var command = new RemoveSlotCommand(nonExistentDoctorId, slotId);

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
    public async Task Handle_SlotNotInSchedule_ThrowsDomainException()
    {
        // Arrange
        var doctorId = UserId.New();
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        _scheduleRepo.Save(schedule);

        var nonExistentSlotId = Guid.NewGuid();
        var command = new RemoveSlotCommand(doctorId.Value, nonExistentSlotId);

        // Act & Assert
        await Assert.ThrowsAsync<SlotNotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SlotWithPlannedAppointment_ThrowsDomainException()
    {
        // Arrange
        var doctorId = UserId.New();
        var patientId = UserId.New();
        var (_, slot) = CreateScheduleWithSlot(doctorId);

        // Create a planned appointment for this slot
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            slot.Id,
            patientId,
            doctorId,
            FutureSlotStart,
            new Money(100));
        _appointmentRepo.Save(appointment);

        var command = new RemoveSlotCommand(doctorId.Value, slot.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Verify slot was not removed
        var unchanged = _scheduleRepo.FindByDoctor(doctorId);
        Assert.Single(unchanged!.Slots);
    }

    [Fact]
    public async Task Handle_SlotWithConfirmedAppointment_ThrowsDomainException()
    {
        // Arrange
        var doctorId = UserId.New();
        var patientId = UserId.New();
        var (_, slot) = CreateScheduleWithSlot(doctorId);

        // Create a confirmed appointment for this slot
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            slot.Id,
            patientId,
            doctorId,
            FutureSlotStart,
            new Money(100));
        appointment.AddPayment(PaymentId.New(), new Money(100), Now); // Confirmed
        _appointmentRepo.Save(appointment);

        var command = new RemoveSlotCommand(doctorId.Value, slot.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SlotWithCancelledAppointment_RemovesSuccessfully()
    {
        // Arrange
        var doctorId = UserId.New();
        var patientId = UserId.New();
        var (_, slot) = CreateScheduleWithSlot(doctorId);

        // Create a cancelled appointment (no longer active)
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            slot.Id,
            patientId,
            doctorId,
            FutureSlotStart,
            new Money(100));
        appointment.Cancel(patientId, Now);
        _appointmentRepo.Save(appointment);

        var command = new RemoveSlotCommand(doctorId.Value, slot.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - cancelled appointments don't block slot removal
        Assert.True(result.IsSuccess);

        var updated = _scheduleRepo.FindByDoctor(doctorId);
        Assert.Empty(updated!.Slots);
    }

    [Fact]
    public async Task Handle_SlotWithCompletedAppointment_RemovesSuccessfully()
    {
        // Arrange
        var doctorId = UserId.New();
        var patientId = UserId.New();
        var (_, slot) = CreateScheduleWithSlot(doctorId);

        // Create a completed appointment (no longer active)
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            slot.Id,
            patientId,
            doctorId,
            FutureSlotStart,
            new Money(100));
        appointment.AddPayment(PaymentId.New(), new Money(100), Now); // Confirmed
        appointment.Complete(); // Completed
        _appointmentRepo.Save(appointment);

        var command = new RemoveSlotCommand(doctorId.Value, slot.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - completed appointments don't block slot removal
        Assert.True(result.IsSuccess);

        var updated = _scheduleRepo.FindByDoctor(doctorId);
        Assert.Empty(updated!.Slots);
    }

    [Fact]
    public async Task Handle_MultipleSlots_RemovesOnlySpecifiedSlot()
    {
        // Arrange
        var doctorId = UserId.New();
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        
        var slot1 = schedule.AddSlot(
            TimeSlotId.New(),
            FutureSlotStart,
            TimeSpan.FromHours(1),
            new Money(100),
            Now);
        
        var slot2 = schedule.AddSlot(
            TimeSlotId.New(),
            FutureSlotStart.AddHours(2),
            TimeSpan.FromHours(1),
            new Money(100),
            Now);
        
        _scheduleRepo.Save(schedule);

        // Remove only slot1
        var command = new RemoveSlotCommand(doctorId.Value, slot1.Id.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = _scheduleRepo.FindByDoctor(doctorId);
        Assert.Single(updated!.Slots);
        Assert.Equal(slot2.Id, updated.Slots[0].Id);
    }
}
