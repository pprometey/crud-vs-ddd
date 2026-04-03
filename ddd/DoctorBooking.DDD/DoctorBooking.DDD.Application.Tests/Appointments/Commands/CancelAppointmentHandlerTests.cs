using Core.Common.Domain;
using DoctorBooking.DDD.Application.Appointments.Commands.CancelAppointment;
using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Appointments.Commands;

public class CancelAppointmentHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureSlotStart = Now.AddDays(3);

    private readonly FakeAppointmentRepository _appointmentRepo;
    private readonly FakeClock _clock;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly CancelAppointmentHandler _handler;

    public CancelAppointmentHandlerTests()
    {
        _appointmentRepo = new FakeAppointmentRepository();
        _clock = new FakeClock(Now);
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();

        _handler = new CancelAppointmentHandler(_appointmentRepo, _uow, _clock, _publisher);
    }

    private AppointmentAgg CreatePlannedAppointment(Guid? appointmentId = null)
    {
        var id = new AppointmentId(appointmentId ?? Guid.NewGuid());
        var patientId = UserId.New();
        var doctorId = UserId.New();
        var slotId = TimeSlotId.New();

        var appointment = new AppointmentAgg(
            id,
            slotId,
            patientId,
            doctorId,
            FutureSlotStart,
            new Money(100));

        _appointmentRepo.Save(appointment);
        return appointment;
    }

    private AppointmentAgg CreateConfirmedAppointment()
    {
        var appointment = CreatePlannedAppointment();
        appointment.AddPayment(PaymentId.New(), new Money(100), Now);
        _appointmentRepo.Save(appointment);
        return appointment;
    }

    [Fact]
    public async Task Handle_ValidCancellation_CancelsAppointment()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();
        var command = new CancelAppointmentCommand(
            AppointmentId: appointment.Id.Value,
            CancelledById: appointment.PatientId.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var cancelled = _appointmentRepo.FindById(appointment.Id);
        Assert.NotNull(cancelled);
        Assert.Equal(AppointmentStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task Handle_ValidCancellation_CallsSaveChanges()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();
        var command = new CancelAppointmentCommand(
            appointment.Id.Value,
            appointment.PatientId.Value);

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
        var command = new CancelAppointmentCommand(nonExistentId, Guid.NewGuid());

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
    public async Task Handle_CancelConfirmedAppointment_Succeeds()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        var command = new CancelAppointmentCommand(
            appointment.Id.Value,
            appointment.DoctorId.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public async Task Handle_CancelAfterAppointmentStarted_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();

        // Advance time to after appointment start
        _clock.Set(FutureSlotStart.AddMinutes(1));

        var command = new CancelAppointmentCommand(
            appointment.Id.Value,
            appointment.PatientId.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Verify appointment was not changed
        var unchanged = _appointmentRepo.FindById(appointment.Id);
        Assert.Equal(AppointmentStatus.Planned, unchanged!.Status);
    }

    [Fact]
    public async Task Handle_CancelAlreadyCancelledAppointment_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreatePlannedAppointment();
        appointment.Cancel(appointment.PatientId, Now);
        _appointmentRepo.Save(appointment);

        var command = new CancelAppointmentCommand(
            appointment.Id.Value,
            appointment.PatientId.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CancelCompletedAppointment_ThrowsDomainException()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        appointment.Complete();
        _appointmentRepo.Save(appointment);

        var command = new CancelAppointmentCommand(
            appointment.Id.Value,
            appointment.DoctorId.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CancelWithRefundWindow_RegistersRefundEvent()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment(); // has payment
        _publisher.Clear();

        // Cancel more than 2 hours before appointment
        _clock.Set(FutureSlotStart.AddHours(-3));

        var command = new CancelAppointmentCommand(
            appointment.Id.Value,
            appointment.PatientId.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var cancelledEvent = _publisher.PublishedEvents.OfType<AppointmentCancelled>().FirstOrDefault();

        Assert.NotNull(cancelledEvent);
        Assert.True(cancelledEvent.ShouldRefund);
    }

    [Fact]
    public async Task Handle_CancelOutsideRefundWindow_NoRefund()
    {
        // Arrange
        var appointment = CreateConfirmedAppointment();
        _publisher.Clear();

        // Cancel less than 2 hours before appointment (1 hour before)
        _clock.Set(FutureSlotStart.AddHours(-1));

        var command = new CancelAppointmentCommand(
            appointment.Id.Value,
            appointment.PatientId.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var cancelledEvent = _publisher.PublishedEvents.OfType<AppointmentCancelled>().FirstOrDefault();

        Assert.NotNull(cancelledEvent);
        Assert.False(cancelledEvent.ShouldRefund);
    }
}
