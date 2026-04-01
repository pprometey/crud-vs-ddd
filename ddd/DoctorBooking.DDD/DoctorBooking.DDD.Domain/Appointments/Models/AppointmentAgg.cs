using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Appointments.Events;
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

        RegisterEvent(new AppointmentCreated(id, patientId, doctorId, slotId));
    }

    public void AddPayment(Money amount, DateTime paidAt)
    {
        Guard.Against.PaymentNotAllowedInStatus(Status);

        var newTotal = PaidTotal() + amount;
        Guard.Against.PaymentExceedsSlotPrice(newTotal, SlotPrice, RemainingBalance());

        var payment = new Payment(PaymentId.New(), amount, paidAt);
        _payments.Add(payment);
        RegisterEvent(new PaymentAdded(Id, payment.Id, amount, paidAt));

        if (newTotal == SlotPrice)
        {
            Status = AppointmentStatus.Confirmed;
            RegisterEvent(new AppointmentConfirmed(Id, PatientId, DoctorId));
        }
    }

    public void ConfirmFree()
    {
        Guard.Against.FreeConfirmWithPrice(SlotPrice);
        Guard.Against.FreeConfirmWrongStatus(Status);

        Status = AppointmentStatus.Confirmed;
        RegisterEvent(new AppointmentConfirmed(Id, PatientId, DoctorId));
    }

    public void Cancel(UserId cancelledBy, DateTime now)
    {
        Guard.Against.AppointmentNotCancellable(Status);
        Guard.Against.AppointmentAlreadyStarted(now, SlotStart);

        var shouldRefund = _payments.Count > 0 && now <= SlotStart.AddHours(-2);
        Status = AppointmentStatus.Cancelled;
        RegisterEvent(new AppointmentCancelled(Id, cancelledBy, shouldRefund, _payments.ToList()));
    }

    public void Complete()
    {
        Guard.Against.NotConfirmedForComplete(Status);

        Status = AppointmentStatus.Completed;
        RegisterEvent(new AppointmentCompleted(Id, DoctorId));
    }

    public void MarkNoShow()
    {
        Guard.Against.NotConfirmedForNoShow(Status);

        Status = AppointmentStatus.Cancelled;
        RegisterEvent(new AppointmentNoShow(Id, PatientId));
    }

    public Money PaidTotal() => _payments.Aggregate(Money.Zero, (acc, p) => acc + p.Amount);
    public Money RemainingBalance() => SlotPrice - PaidTotal();
}
