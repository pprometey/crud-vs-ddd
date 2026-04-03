using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Appointments;

public sealed class AppointmentAgg : AggregateRoot<AppointmentId>
{
    private readonly List<Payment> _payments = [];

    public TimeSlotId SlotId { get; private set; }
    public UserId PatientId { get; private set; }
    public UserId DoctorId { get; private set; }
    public DateTime SlotStart { get; private set; }
    public Money SlotPrice { get; private set; }
    public AppointmentStatus Status { get; private set; }

    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();

    // EF Core constructor
    private AppointmentAgg() : base(default!)
    {
        SlotId = default!;
        PatientId = default!;
        DoctorId = default!;
        SlotPrice = default!;
    }

    public AppointmentAgg(
        AppointmentId id,
        TimeSlotId slotId,
        UserId patientId,
        UserId doctorId,
        DateTime slotStart,
        Money slotPrice) : base(id)
    {
        Guard.Against.PatientIsOwnDoctor(patientId, doctorId);

        SlotId = slotId;
        PatientId = patientId;
        DoctorId = doctorId;
        SlotStart = slotStart;
        SlotPrice = slotPrice;
        Status = AppointmentStatus.Planned;
    }

    public Payment AddPayment(PaymentId paymentId, Money amount, DateTime paidAt)
    {
        Guard.Against.PaymentNotAllowedInStatus(Status);

        var newTotal = PaidTotal() + amount;
        Guard.Against.PaymentExceedsSlotPrice(newTotal, SlotPrice, RemainingBalance());

        var payment = new Payment(paymentId, amount, paidAt);
        _payments.Add(payment);

        if (newTotal == SlotPrice)
        {
            Status = AppointmentStatus.Confirmed;
        }

        return payment;
    }

    public void ConfirmFree()
    {
        Guard.Against.FreeConfirmWithPrice(SlotPrice);
        Guard.Against.FreeConfirmWrongStatus(Status);

        Status = AppointmentStatus.Confirmed;
    }

    public void Cancel(UserId cancelledBy, DateTime now)
    {
        Guard.Against.AppointmentNotCancellable(Status);
        Guard.Against.AppointmentAlreadyStarted(now, SlotStart);
        Status = AppointmentStatus.Cancelled;
    }

    public bool ShouldRefund(DateTime now)
    {
        if (_payments.Count == 0)
            return false;
            
        var timeUntilAppointment = SlotStart - now;
        return timeUntilAppointment.TotalHours > 2;
    }

    public void Complete()
    {
        Guard.Against.NotConfirmedForComplete(Status);
        Status = AppointmentStatus.Completed;
    }

    public void MarkNoShow()
    {
        Guard.Against.NotConfirmedForNoShow(Status);
        Status = AppointmentStatus.Cancelled;
    }

    public Money PaidTotal() => _payments.Aggregate(Money.Zero, (acc, p) => acc + p.Amount);
    public Money RemainingBalance() => SlotPrice - PaidTotal();
}
