using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Services;
using DoctorBooking.DDD.Domain.Tests.Fakes;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Domain.Tests.Services;

public class AppointmentBookingServiceTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureSlotStart = Now.AddDays(3);
    private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

    private static (AppointmentBookingService service,
             FakeScheduleRepository scheduleRepo,
             FakeAppointmentRepository appointmentRepo,
             FakeUserRepository userRepo,
             FakeClock clock) CreateSut()
    {
        var scheduleRepo = new FakeScheduleRepository();
        var appointmentRepo = new FakeAppointmentRepository();
        var userRepo = new FakeUserRepository();
        var clock = new FakeClock(Now);
        var service = new AppointmentBookingService(scheduleRepo, appointmentRepo, userRepo, clock);
        return (service, scheduleRepo, appointmentRepo, userRepo, clock);
    }

    private static UserAgg CreatePatient(UserId id)
        => new(id, new Email("patient@test.com"), new PersonName("Jane", "Doe"));

    private static (ScheduleAgg schedule, TimeSlot slot) CreateScheduleWithSlot(
        FakeScheduleRepository repo,
        UserId doctorId,
        Money? price = null,
        DateTime? start = null)
    {
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        var slotId = TimeSlotId.New();
        var slot = schedule.AddSlot(
            slotId,
            start ?? FutureSlotStart,
            OneHour,
            price ?? new Money(100),
            Now.AddDays(-10)); // "now" in the past so slot is valid
        repo.Save(schedule);
        return (schedule, slot);
    }

    // ── UserAgg validation ───────────────────────────────────────────────────────

    [Fact]
    public void Book_UserNotFound_ThrowsDomainException()
    {
        var (service, _, _, _, _) = CreateSut();
        var missingUserId = UserId.New();

        Assert.Throws<DomainException>(() =>
            service.Book(missingUserId, TimeSlotId.New()));
    }

    [Fact]
    public void Book_UserWithoutPatientRole_ThrowsDomainException()
    {
        var (service, scheduleRepo, _, userRepo, _) = CreateSut();

        var doctorId = UserId.New();
        // Create a doctor-only user
        var doctor = new UserAgg(doctorId, new Email("doc@test.com"), new PersonName("Dr", "Smith"), UserRole.Doctor);
        userRepo.Save(doctor);

        var (_, slot) = CreateScheduleWithSlot(scheduleRepo, UserId.New());

        Assert.Throws<DomainException>(() => service.Book(doctorId, slot.Id));
    }

    // ── Slot validation ───────────────────────────────────────────────────────

    [Fact]
    public void Book_SlotNotFound_ThrowsSlotNotFoundException()
    {
        var (service, _, _, userRepo, _) = CreateSut();
        var patientId = UserId.New();
        userRepo.Save(CreatePatient(patientId));

        Assert.Throws<SlotNotFoundException>(() =>
            service.Book(patientId, TimeSlotId.New()));
    }

    [Fact]
    public void Book_UserIsOwnerOfSlot_ThrowsDomainException()
    {
        var (service, scheduleRepo, _, userRepo, _) = CreateSut();

        var userId = UserId.New();
        // UserAgg is both patient and doctor (but tries to book their own slot)
        var user = new UserAgg(userId, new Email("both@test.com"), new PersonName("Dr", "Double"));
        user.AddRole(UserRole.Doctor);
        userRepo.Save(user);

        // ScheduleAgg owned by this same user
        var schedule = new ScheduleAgg(ScheduleId.New(), userId);
        var slotId = TimeSlotId.New();
        schedule.AddSlot(slotId, FutureSlotStart, OneHour, new Money(50), Now.AddDays(-1));
        scheduleRepo.Save(schedule);

        Assert.Throws<DomainException>(() => service.Book(userId, slotId));
    }

    [Fact]
    public void Book_PastSlot_ThrowsDomainException()
    {
        var (service, scheduleRepo, _, userRepo, clock) = CreateSut();

        var patientId = UserId.New();
        var doctorId = UserId.New();
        userRepo.Save(CreatePatient(patientId));

        // Add a slot in the past relative to the current clock
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        var slotId = TimeSlotId.New();
        schedule.AddSlot(slotId, Now.AddHours(2), OneHour, new Money(100), Now.AddDays(-1));
        scheduleRepo.Save(schedule);

        // Move clock forward so the slot is now in the past
        clock.UtcNow = Now.AddHours(3);

        Assert.Throws<DomainException>(() => service.Book(patientId, slotId));
    }

    [Fact]
    public void Book_SlotAlreadyConfirmed_ThrowsDomainException()
    {
        var (service, scheduleRepo, appointmentRepo, userRepo, _) = CreateSut();

        var patient1Id = UserId.New();
        var patient2Id = UserId.New();
        var doctorId = UserId.New();
        userRepo.Save(CreatePatient(patient1Id));
        userRepo.Save(CreatePatient(patient2Id));

        var (_, slot) = CreateScheduleWithSlot(scheduleRepo, doctorId);

        // First patient books and pays → slot becomes CONFIRMED
        var existing = new AppointmentAgg(AppointmentId.New(), slot.Id, patient1Id, doctorId, FutureSlotStart, new Money(100));
        existing.AddPayment(PaymentId.New(), new Money(100), Now);
        appointmentRepo.Save(existing);

        Assert.Throws<DomainException>(() => service.Book(patient2Id, slot.Id));
    }

    // ── Happy paths ───────────────────────────────────────────────────────────

    [Fact]
    public void Book_ValidRequest_ReturnsPlannedAppointment()
    {
        var (service, scheduleRepo, appointmentRepo, userRepo, _) = CreateSut();

        var patientId = UserId.New();
        var doctorId = UserId.New();
        userRepo.Save(CreatePatient(patientId));
        var (_, slot) = CreateScheduleWithSlot(scheduleRepo, doctorId);

        var appointment = service.Book(patientId, slot.Id);

        Assert.Equal(AppointmentStatus.Planned, appointment.Status);
        Assert.Equal(patientId, appointment.PatientId);
        Assert.Equal(slot.Id, appointment.SlotId);
        Assert.NotNull(appointmentRepo.FindById(appointment.Id));
    }

    [Fact]
    public void Book_FreeSlot_AppointmentImmediatelyConfirmed()
    {
        var (service, scheduleRepo, _, userRepo, _) = CreateSut();

        var patientId = UserId.New();
        var doctorId = UserId.New();
        userRepo.Save(CreatePatient(patientId));
        var (_, slot) = CreateScheduleWithSlot(scheduleRepo, doctorId, price: Money.Zero);

        var appointment = service.Book(patientId, slot.Id);

        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
    }

    [Fact]
    public void Book_MultiplePatients_CanBookSamePlannedSlot()
    {
        // Two patients can both be PLANNED on the same slot; first to pay wins
        var (service, scheduleRepo, _, userRepo, _) = CreateSut();

        var patient1 = UserId.New();
        var patient2 = UserId.New();
        var doctorId = UserId.New();
        userRepo.Save(CreatePatient(patient1));
        userRepo.Save(CreatePatient(patient2));
        var (_, slot) = CreateScheduleWithSlot(scheduleRepo, doctorId);

        var appt1 = service.Book(patient1, slot.Id);
        var appt2 = service.Book(patient2, slot.Id); // should succeed — no confirmed booking yet

        Assert.Equal(AppointmentStatus.Planned, appt1.Status);
        Assert.Equal(AppointmentStatus.Planned, appt2.Status);
    }
}
