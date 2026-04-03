using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Domain.Tests.Appointments;

public class AppointmentTests
{
    private static readonly DateTime SlotStart = new(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly UserId PatientId = UserId.New();
    private static readonly UserId DoctorId = UserId.New();
    private static readonly TimeSlotId SlotId = TimeSlotId.New();
    private static readonly Money SlotPrice = new(100);

    private static AppointmentAgg CreatePlanned(Money? price = null)
        => new(AppointmentId.New(), SlotId, PatientId, DoctorId, SlotStart, price ?? SlotPrice);

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_PatientEqualsDoctor_ThrowsDomainException()
    {
        var sameId = UserId.New();
        Assert.Throws<DomainException>(() =>
            new AppointmentAgg(AppointmentId.New(), SlotId, sameId, sameId, SlotStart, SlotPrice));
    }

    [Fact]
    public void Constructor_ValidInput_StatusIsPlanned()
    {
        var appt = CreatePlanned();
        Assert.Equal(AppointmentStatus.Planned, appt.Status);
    }

    // ── AddPayment ────────────────────────────────────────────────────────────

    [Fact]
    public void AddPayment_WhenPlanned_Succeeds()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), new Money(50), DateTime.UtcNow);

        Assert.Equal(new Money(50), appt.PaidTotal());
        Assert.Equal(AppointmentStatus.Planned, appt.Status); // still planned (partial)
    }

    [Fact]
    public void AddPayment_FullAmount_TransitionsToConfirmed()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), SlotPrice, DateTime.UtcNow);

        Assert.Equal(AppointmentStatus.Confirmed, appt.Status);
    }

    [Fact]
    public void AddPayment_MultipleParts_ConfirmedWhenFull()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), new Money(60), DateTime.UtcNow);
        Assert.Equal(AppointmentStatus.Planned, appt.Status);

        appt.AddPayment(PaymentId.New(), new Money(40), DateTime.UtcNow);
        Assert.Equal(AppointmentStatus.Confirmed, appt.Status);
        Assert.Equal(new Money(100), appt.PaidTotal());
    }

    [Fact]
    public void AddPayment_WouldExceedSlotPrice_ThrowsDomainException()
    {
        var appt = CreatePlanned();
        Assert.Throws<DomainException>(() =>
            appt.AddPayment(PaymentId.New(), new Money(150), DateTime.UtcNow));
    }

    [Fact]
    public void AddPayment_WhenConfirmed_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), SlotPrice, DateTime.UtcNow); // → Confirmed

        Assert.Throws<DomainException>(() =>
            appt.AddPayment(PaymentId.New(), new Money(1), DateTime.UtcNow));
    }

    [Fact]
    public void AddPayment_WhenCancelled_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.Cancel(PatientId, SlotStart.AddHours(-3));

        Assert.Throws<DomainException>(() =>
            appt.AddPayment(PaymentId.New(), new Money(10), DateTime.UtcNow));
    }

    // ── ConfirmFree ───────────────────────────────────────────────────────────

    [Fact]
    public void ConfirmFree_ZeroPrice_TransitionsToConfirmed()
    {
        var appt = CreatePlanned(price: Money.Zero);
        appt.ConfirmFree();

        Assert.Equal(AppointmentStatus.Confirmed, appt.Status);
    }

    [Fact]
    public void ConfirmFree_NonZeroPrice_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        Assert.Throws<DomainException>(() => appt.ConfirmFree());
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_FromPlanned_BeforeStart_Succeeds()
    {
        var appt = CreatePlanned();
        appt.Cancel(PatientId, SlotStart.AddHours(-1));

        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
    }

    [Fact]
    public void Cancel_FromConfirmed_BeforeStart_Succeeds()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), SlotPrice, DateTime.UtcNow);
        appt.Cancel(PatientId, SlotStart.AddHours(-3));

        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
    }

    [Fact]
    public void Cancel_AtOrAfterStart_ThrowsDomainException()
    {
        var appt = CreatePlanned();
        Assert.Throws<DomainException>(() => appt.Cancel(PatientId, SlotStart));
        Assert.Throws<DomainException>(() => appt.Cancel(PatientId, SlotStart.AddMinutes(1)));
    }

    [Fact]
    public void Cancel_WithPaymentMoreThan2HoursBefore_Succeeds()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), new Money(50), DateTime.UtcNow);

        appt.Cancel(PatientId, SlotStart.AddHours(-3));

        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
    }

    [Fact]
    public void Cancel_WithPaymentLessThan2HoursBefore_Succeeds()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), new Money(50), DateTime.UtcNow);

        appt.Cancel(PatientId, SlotStart.AddMinutes(-119)); // 1h 59m before

        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
    }

    [Fact]
    public void Cancel_WithoutPayment_Succeeds()
    {
        var appt = CreatePlanned();

        appt.Cancel(PatientId, SlotStart.AddHours(-5));

        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
    }

    [Fact]
    public void Cancel_FromCompleted_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), SlotPrice, DateTime.UtcNow);
        appt.Complete();

        Assert.Throws<DomainException>(() =>
            appt.Cancel(PatientId, SlotStart.AddHours(-1)));
    }

    // ── Complete ──────────────────────────────────────────────────────────────

    [Fact]
    public void Complete_FromConfirmed_Succeeds()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), SlotPrice, DateTime.UtcNow);

        appt.Complete();

        Assert.Equal(AppointmentStatus.Completed, appt.Status);
    }

    [Fact]
    public void Complete_FromPlanned_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        Assert.Throws<DomainException>(() => appt.Complete());
    }

    [Fact]
    public void Complete_FromCancelled_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.Cancel(PatientId, SlotStart.AddHours(-1));

        Assert.Throws<DomainException>(() => appt.Complete());
    }

    // ── MarkNoShow ────────────────────────────────────────────────────────────

    [Fact]
    public void MarkNoShow_FromConfirmed_TransitionsToCancelled()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), SlotPrice, DateTime.UtcNow);

        appt.MarkNoShow();

        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
    }

    [Fact]
    public void MarkNoShow_FromPlanned_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        Assert.Throws<DomainException>(() => appt.MarkNoShow());
    }

    // ── RemainingBalance ──────────────────────────────────────────────────────

    [Fact]
    public void RemainingBalance_NoPayments_EqualsTotalPrice()
    {
        var appt = CreatePlanned();
        Assert.Equal(SlotPrice, appt.RemainingBalance());
    }

    [Fact]
    public void RemainingBalance_PartialPayment_Correct()
    {
        var appt = CreatePlanned();
        appt.AddPayment(PaymentId.New(), new Money(30), DateTime.UtcNow);
        Assert.Equal(new Money(70), appt.RemainingBalance());
    }
}
