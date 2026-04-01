using Ardalis.GuardClauses;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Errors;

namespace DoctorBooking.DDD.Domain.Appointments;

public sealed class Payment : Entity<PaymentId>
{
    public Money Amount { get; private set; }
    public DateTime PaidAt { get; private set; }
    public PaymentStatus Status { get; private set; }

    public Payment(PaymentId id, Money amount, DateTime paidAt) : base(id)
    {
        Guard.Against.ZeroPaymentAmount(amount.Amount);

        Amount = amount;
        PaidAt = paidAt;
        Status = PaymentStatus.Paid;
    }
}
