using Core.Common.Domain;
using DoctorBooking.DDD.Application.Appointments.Commands.AddPayment;
using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Appointments.Commands;

public class AddPaymentHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureSlotStart = Now.AddDays(3);

    private readonly FakeAppointmentRepository _appointmentRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly AddPaymentHandler _handler;

    public AddPaymentHandlerTests()
    {
        _appointmentRepo = new FakeAppointmentRepository();
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();
        _handler = new AddPaymentHandler(_appointmentRepo, _uow, _publisher);
    }

    private AppointmentAgg CreatePlannedAppointment(Money? price = null)
    {
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            FutureSlotStart,
            price ?? new Money(100));

        _appointmentRepo.Save(appointment);
        return appointment;
    }

    [Fact]
    public async Task Handle_ValidPayment_AddsPaymentAndReturnsPaymentId()
    {
        // Arrange
        var appointment = CreatePlannedAppointment(price: new Money(100));
        var command = new AddPaymentCommand(
            AppointmentId: appointment.Id.Value,
            Amount: 50m,
            PaidAt: Now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var updated = _appointmentRepo.FindById(appointment.Id);
        Assert.NotNull(updated);
        Assert.Single(updated.Payments);
        Assert.Equal(new Money(50), updated.Payments[0].Amount);
        Assert.Equal(new Money(50), updated.PaidTotal());
        Assert.Equal(AppointmentStatus.Planned, updated.Status); // still planned (partial)
    }

    [Fact]
    public async Task Handle_ValidPayment_CallsSaveChanges()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();
        var command = new AddPaymentCommand(appointment.Id.Value, 50m, Now);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_FullPayment_ConfirmsAppointment()
    {
        // Arrange
        var appointment = CreatePlannedAppointment(price: new Money(100));
        var command = new AddPaymentCommand(
            appointment.Id.Value,
            Amount: 100m,
            PaidAt: Now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = _appointmentRepo.FindById(appointment.Id);
        Assert.NotNull(updated);
        Assert.Equal(AppointmentStatus.Confirmed, updated.Status);
        Assert.Equal(new Money(100), updated.PaidTotal());
    }

    [Fact]
    public async Task Handle_MultiplePartialPayments_EventuallyConfirms()
    {
        // Arrange
        var appointment = CreatePlannedAppointment(price: new Money(100));

        // First payment: 60
        var command1 = new AddPaymentCommand(appointment.Id.Value, 60m, Now);
        await _handler.Handle(command1, CancellationToken.None);

        var afterFirst = _appointmentRepo.FindById(appointment.Id);
        Assert.Equal(AppointmentStatus.Planned, afterFirst!.Status);

        // Second payment: 40
        var command2 = new AddPaymentCommand(appointment.Id.Value, 40m, Now);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.True(result2.IsSuccess);

        var updated = _appointmentRepo.FindById(appointment.Id);
        Assert.NotNull(updated);
        Assert.Equal(2, updated.Payments.Count);
        Assert.Equal(new Money(100), updated.PaidTotal());
        Assert.Equal(AppointmentStatus.Confirmed, updated.Status);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ReturnsFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AddPaymentCommand(nonExistentId, 50m, Now);

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
    public async Task Handle_PaymentExceedsSlotPrice_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment(price: new Money(100));
        var command = new AddPaymentCommand(appointment.Id.Value, 150m, Now);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Verify appointment was not changed
        var unchanged = _appointmentRepo.FindById(appointment.Id);
        Assert.Empty(unchanged!.Payments);
    }

    [Fact]
    public async Task Handle_PaymentOnConfirmedAppointment_ThrowsInvalidOperationException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment(price: new Money(100));
        appointment.AddPayment(PaymentId.New(), new Money(100), Now); // full payment → Confirmed
        _appointmentRepo.Save(appointment);

        var command = new AddPaymentCommand(appointment.Id.Value, 10m, Now);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PaymentOnCancelledAppointment_ThrowsInvalidOperationException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();
        appointment.Cancel(appointment.PatientId, Now);
        _appointmentRepo.Save(appointment);

        var command = new AddPaymentCommand(appointment.Id.Value, 50m, Now);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NegativeAmount_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();
        var command = new AddPaymentCommand(appointment.Id.Value, -50m, Now);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ZeroAmount_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();
        var command = new AddPaymentCommand(appointment.Id.Value, 0m, Now);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }
}
