using Core.Common.Domain;
using DoctorBooking.DDD.Application.Appointments.Commands.CompleteAppointment;
using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Appointments.Commands;

public class CompleteAppointmentHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureSlotStart = Now.AddDays(3);

    private readonly FakeAppointmentRepository _appointmentRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly CompleteAppointmentHandler _handler;

    public CompleteAppointmentHandlerTests()
    {
        _appointmentRepo = new FakeAppointmentRepository();
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();
        _handler = new CompleteAppointmentHandler(_appointmentRepo, _uow, _publisher);
    }

    private AppointmentAgg CreateConfirmedAppointment(Guid? appointmentId = null)
    {
        var id = new AppointmentId(appointmentId ?? Guid.NewGuid());
        var appointment = new AppointmentAgg(
            id,
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            FutureSlotStart,
            new Money(100));

        appointment.AddPayment(PaymentId.New(), new Money(100), Now); // Full payment → Confirmed
        _appointmentRepo.Save(appointment);
        return appointment;
    }

    private AppointmentAgg CreatePlannedAppointment()
    {
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            FutureSlotStart,
            new Money(100));

        _appointmentRepo.Save(appointment);
        return appointment;
    }

    [Fact]
    public async Task Handle_ConfirmedAppointment_CompletesSuccessfully()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        var command = new CompleteAppointmentCommand(appointment.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var completed = _appointmentRepo.FindById(appointment.Id);
        Assert.NotNull(completed);
        Assert.Equal(AppointmentStatus.Completed, completed.Status);
    }

    [Fact]
    public async Task Handle_ValidCompletion_CallsSaveChanges()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        var command = new CompleteAppointmentCommand(appointment.Id.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ReturnsFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new CompleteAppointmentCommand(nonExistentId);

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
    public async Task Handle_PlannedAppointment_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment(); // Not confirmed
        var command = new CompleteAppointmentCommand(appointment.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Verify appointment status unchanged
        var unchanged = _appointmentRepo.FindById(appointment.Id);
        Assert.Equal(AppointmentStatus.Planned, unchanged!.Status);
    }

    [Fact]
    public async Task Handle_CancelledAppointment_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        appointment.Cancel(appointment.PatientId, Now);
        _appointmentRepo.Save(appointment);

        var command = new CompleteAppointmentCommand(appointment.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyCompletedAppointment_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        appointment.Complete();
        _appointmentRepo.Save(appointment);

        var command = new CompleteAppointmentCommand(appointment.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Completion_RegistersAppointmentCompletedEvent()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        _publisher.Clear();

        var command = new CompleteAppointmentCommand(appointment.Id.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var completedEvent = _publisher.PublishedEvents.OfType<AppointmentCompleted>().FirstOrDefault();

        Assert.NotNull(completedEvent);
        Assert.Equal(appointment.Id, completedEvent.AppointmentId);
        Assert.Equal(appointment.DoctorId, completedEvent.DoctorId);
    }
}
