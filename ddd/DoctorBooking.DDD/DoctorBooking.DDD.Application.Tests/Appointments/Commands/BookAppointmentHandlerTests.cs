using Core.Common.Domain;
using DoctorBooking.DDD.Application.Appointments.Commands.BookAppointment;
using DoctorBooking.DDD.Application.Tests.Fakes;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Services;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Appointments.Commands;

public class BookAppointmentHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureSlotStart = Now.AddDays(3); // June 4th, 9am

    private readonly FakeScheduleRepository _scheduleRepo;
    private readonly FakeAppointmentRepository _appointmentRepo;
    private readonly FakeUserRepository _userRepo;
    private readonly FakeClock _clock;
    private readonly FakeUnitOfWork _uow;
    private readonly FakePublisher _publisher;
    private readonly BookAppointmentHandler _handler;

    public BookAppointmentHandlerTests()
    {
        _scheduleRepo = new FakeScheduleRepository();
        _appointmentRepo = new FakeAppointmentRepository();
        _userRepo = new FakeUserRepository();
        _clock = new FakeClock(Now);
        _uow = new FakeUnitOfWork();
        _publisher = new FakePublisher();

        var bookingService = new AppointmentBookingService(
            _scheduleRepo,
            _appointmentRepo,
            _userRepo,
            _clock);

        _handler = new BookAppointmentHandler(bookingService, _uow, _publisher);
    }

    private UserAgg CreatePatient(Guid? patientId = null)
    {
        var id = new UserId(patientId ?? Guid.NewGuid());
        var patient = new UserAgg(
            id,
            new Email($"patient-{id.Value}@test.com"),
            new PersonName("Jane", "Doe"));
        _userRepo.Save(patient);
        return patient;
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

    private TimeSlot CreateSlotForDoctor(UserId doctorId, Money? price = null, DateTime? start = null)
    {
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        var slotId = TimeSlotId.New();
        var slot = schedule.AddSlot(
            slotId,
            start ?? FutureSlotStart,
            TimeSpan.FromHours(1),
            price ?? new Money(100),
            Now.AddDays(-10)); // created in the past
        _scheduleRepo.Save(schedule);
        return slot;
    }

    [Fact]
    public async Task Handle_ValidBooking_CreatesAppointmentAndReturnsId()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = CreateDoctor();
        var slot = CreateSlotForDoctor(doctor.Id);

        var command = new BookAppointmentCommand(
            PatientId: patient.Id.Value,
            SlotId: slot.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var appointment = _appointmentRepo.FindById(new AppointmentId(result.Value));
        Assert.NotNull(appointment);
        Assert.Equal(patient.Id, appointment.PatientId);
        Assert.Equal(doctor.Id, appointment.DoctorId);
        Assert.Equal(slot.Id, appointment.SlotId);
        Assert.Equal(AppointmentStatus.Planned, appointment.Status);
    }

    [Fact]
    public async Task Handle_ValidBooking_CallsSaveChanges()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = CreateDoctor();
        var slot = CreateSlotForDoctor(doctor.Id);

        var command = new BookAppointmentCommand(patient.Id.Value, slot.Id.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_FreeSlot_CreatesConfirmedAppointment()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = CreateDoctor();
        var slot = CreateSlotForDoctor(doctor.Id, price: Money.Zero);

        var command = new BookAppointmentCommand(patient.Id.Value, slot.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var appointment = _appointmentRepo.FindById(new AppointmentId(result.Value));
        Assert.NotNull(appointment);
        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
    }

    [Fact]
    public async Task Handle_PatientNotFound_ThrowsDomainException()
    {
        // Arrange
        var doctor = CreateDoctor();
        var slot = CreateSlotForDoctor(doctor.Id);

        var nonExistentPatientId = Guid.NewGuid();
        var command = new BookAppointmentCommand(nonExistentPatientId, slot.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Verify no changes were saved
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_UserIsNotPatient_ThrowsDomainException()
    {
        // Arrange
        var doctor = CreateDoctor(); // has Doctor role, not Patient
        var anotherDoctor = CreateDoctor();
        var slot = CreateSlotForDoctor(anotherDoctor.Id);

        var command = new BookAppointmentCommand(doctor.Id.Value, slot.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SlotNotFound_ThrowsSlotNotFoundException()
    {
        // Arrange
        var patient = CreatePatient();
        var nonExistentSlotId = Guid.NewGuid();

        var command = new BookAppointmentCommand(patient.Id.Value, nonExistentSlotId);

        // Act & Assert
        await Assert.ThrowsAsync<SlotNotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PatientTriesToBookOwnSlot_ThrowsDomainException()
    {
        // Arrange
        var doctorPatient = CreatePatient();
        doctorPatient.AddRole(UserRole.Doctor); // user is both patient and doctor
        _userRepo.Save(doctorPatient);

        var slot = CreateSlotForDoctor(doctorPatient.Id);

        var command = new BookAppointmentCommand(doctorPatient.Id.Value, slot.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SlotInPast_ThrowsDomainException()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = CreateDoctor();

        var pastSlotStart = Now.AddDays(-1); // yesterday
        var slot = CreateSlotForDoctor(doctor.Id, start: pastSlotStart);

        var command = new BookAppointmentCommand(patient.Id.Value, slot.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SlotAlreadyConfirmed_ThrowsDomainException()
    {
        // Arrange
        var patient1 = CreatePatient();
        var patient2 = CreatePatient();
        var doctor = CreateDoctor();
        var slot = CreateSlotForDoctor(doctor.Id, price: Money.Zero); // free = auto-confirmed

        // First booking (auto-confirmed because free)
        await _handler.Handle(
            new BookAppointmentCommand(patient1.Id.Value, slot.Id.Value),
            CancellationToken.None);

        // Second patient tries to book the same slot
        var command2 = new BookAppointmentCommand(patient2.Id.Value, slot.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            async () => await _handler.Handle(command2, CancellationToken.None));
    }
}
