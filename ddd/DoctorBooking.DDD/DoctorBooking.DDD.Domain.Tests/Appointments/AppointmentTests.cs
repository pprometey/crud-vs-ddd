using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Appointments.Events;
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

    [Fact]
    public void Constructor_RegistersAppointmentCreatedEvent()
    {
        var appt = CreatePlanned();
        var events = appt.PopDomainEvents();
        Assert.Single(events);
        Assert.IsType<AppointmentCreated>(events[0]);
    }

    // ── AddPayment ────────────────────────────────────────────────────────────

    [Fact]
    public void AddPayment_WhenPlanned_Succeeds()
    {
        var appt = CreatePlanned();
        appt.AddPayment(new Money(50), DateTime.UtcNow);

        Assert.Equal(new Money(50), appt.PaidTotal());
        Assert.Equal(AppointmentStatus.Planned, appt.Status); // still planned (partial)
    }

    [Fact]
    public void AddPayment_FullAmount_TransitionsToConfirmed()
    {
        var appt = CreatePlanned();
        appt.AddPayment(SlotPrice, DateTime.UtcNow);

        Assert.Equal(AppointmentStatus.Confirmed, appt.Status);
    }

    [Fact]
    public void AddPayment_MultipleParts_ConfirmedWhenFull()
    {
        var appt = CreatePlanned();
        appt.AddPayment(new Money(60), DateTime.UtcNow);
        Assert.Equal(AppointmentStatus.Planned, appt.Status);

        appt.AddPayment(new Money(40), DateTime.UtcNow);
        Assert.Equal(AppointmentStatus.Confirmed, appt.Status);
        Assert.Equal(new Money(100), appt.PaidTotal());
    }

    [Fact]
    public void AddPayment_WouldExceedSlotPrice_ThrowsDomainException()
    {
        var appt = CreatePlanned();
        Assert.Throws<DomainException>(() =>
            appt.AddPayment(new Money(150), DateTime.UtcNow));
    }

    [Fact]
    public void AddPayment_WhenConfirmed_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.AddPayment(SlotPrice, DateTime.UtcNow); // → Confirmed

        Assert.Throws<InvalidOperationException>(() =>
            appt.AddPayment(new Money(1), DateTime.UtcNow));
    }

    [Fact]
    public void AddPayment_WhenCancelled_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.Cancel(PatientId, SlotStart.AddHours(-3));

        Assert.Throws<InvalidOperationException>(() =>
            appt.AddPayment(new Money(10), DateTime.UtcNow));
    }

    [Fact]
    public void AddPayment_RegistersPaymentAddedEvent()
    {
        var appt = CreatePlanned();
        appt.PopDomainEvents();

        appt.AddPayment(new Money(50), DateTime.UtcNow);

        var events = appt.PopDomainEvents();
        Assert.Single(events);
        Assert.IsType<PaymentAdded>(events[0]);
    }

    [Fact]
    public void AddPayment_Full_RegistersPaymentAddedAndConfirmedEvents()
    {
        var appt = CreatePlanned();
        appt.PopDomainEvents();

        appt.AddPayment(SlotPrice, DateTime.UtcNow);

        var events = appt.PopDomainEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<PaymentAdded>(events[0]);
        Assert.IsType<AppointmentConfirmed>(events[1]);
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
        Assert.Throws<InvalidOperationException>(() => appt.ConfirmFree());
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
        appt.AddPayment(SlotPrice, DateTime.UtcNow);
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
    public void Cancel_WithPaymentMoreThan2HoursBefore_ShouldRefundIsTrue()
    {
        var appt = CreatePlanned();
        appt.AddPayment(new Money(50), DateTime.UtcNow);
        appt.PopDomainEvents();

        appt.Cancel(PatientId, SlotStart.AddHours(-3));

        var events = appt.PopDomainEvents();
        var cancelled = Assert.IsType<AppointmentCancelled>(events[0]);
        Assert.True(cancelled.ShouldRefund);
        Assert.NotEmpty(cancelled.Payments);
    }

    [Fact]
    public void Cancel_WithPaymentExactly2HoursBefore_ShouldRefundIsTrue()
    {
        var appt = CreatePlanned();
        appt.AddPayment(new Money(50), DateTime.UtcNow);
        appt.PopDomainEvents();

        appt.Cancel(PatientId, SlotStart.AddHours(-2));

        var cancelled = Assert.IsType<AppointmentCancelled>(appt.PopDomainEvents()[0]);
        Assert.True(cancelled.ShouldRefund);
    }

    [Fact]
    public void Cancel_WithPaymentLessThan2HoursBefore_ShouldRefundIsFalse()
    {
        var appt = CreatePlanned();
        appt.AddPayment(new Money(50), DateTime.UtcNow);
        appt.PopDomainEvents();

        appt.Cancel(PatientId, SlotStart.AddMinutes(-119)); // 1h 59m before

        var cancelled = Assert.IsType<AppointmentCancelled>(appt.PopDomainEvents()[0]);
        Assert.False(cancelled.ShouldRefund);
    }

    [Fact]
    public void Cancel_WithoutPayment_ShouldRefundIsFalse()
    {
        var appt = CreatePlanned();
        appt.PopDomainEvents();

        appt.Cancel(PatientId, SlotStart.AddHours(-5));

        var cancelled = Assert.IsType<AppointmentCancelled>(appt.PopDomainEvents()[0]);
        Assert.False(cancelled.ShouldRefund);
        Assert.Empty(cancelled.Payments);
    }

    [Fact]
    public void Cancel_FromCompleted_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.AddPayment(SlotPrice, DateTime.UtcNow);
        appt.Complete();

        Assert.Throws<InvalidOperationException>(() =>
            appt.Cancel(PatientId, SlotStart.AddHours(-1)));
    }

    // ── Complete ──────────────────────────────────────────────────────────────

    [Fact]
    public void Complete_FromConfirmed_Succeeds()
    {
        var appt = CreatePlanned();
        appt.AddPayment(SlotPrice, DateTime.UtcNow);
        appt.PopDomainEvents();

        appt.Complete();

        Assert.Equal(AppointmentStatus.Completed, appt.Status);
        var events = appt.PopDomainEvents();
        Assert.IsType<AppointmentCompleted>(events[0]);
    }

    [Fact]
    public void Complete_FromPlanned_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        Assert.Throws<InvalidOperationException>(() => appt.Complete());
    }

    [Fact]
    public void Complete_FromCancelled_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        appt.Cancel(PatientId, SlotStart.AddHours(-1));

        Assert.Throws<InvalidOperationException>(() => appt.Complete());
    }

    // ── MarkNoShow ────────────────────────────────────────────────────────────

    [Fact]
    public void MarkNoShow_FromConfirmed_TransitionsToCancelled()
    {
        var appt = CreatePlanned();
        appt.AddPayment(SlotPrice, DateTime.UtcNow);
        appt.PopDomainEvents();

        appt.MarkNoShow();

        Assert.Equal(AppointmentStatus.Cancelled, appt.Status);
        var events = appt.PopDomainEvents();
        Assert.IsType<AppointmentNoShow>(events[0]);
    }

    [Fact]
    public void MarkNoShow_FromPlanned_ThrowsInvalidOperation()
    {
        var appt = CreatePlanned();
        Assert.Throws<InvalidOperationException>(() => appt.MarkNoShow());
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
        appt.AddPayment(new Money(30), DateTime.UtcNow);
        Assert.Equal(new Money(70), appt.RemainingBalance());
    }
}
