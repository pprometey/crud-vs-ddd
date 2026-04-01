using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Errors;

namespace DoctorBooking.DDD.Domain.Appointments;

public readonly record struct Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Guard.Against.NegativeMoney(amount);
        Amount = amount;
    }

    public static Money Zero => new(0);

    public bool IsZero() => Amount == 0m;

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);

    public static Money operator -(Money a, Money b)
    {
        Guard.Against.NegativeMoneySubtraction(a.Amount, b.Amount);
        return new(a.Amount - b.Amount);
    }

    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
    public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;
    public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;

    public override string ToString() => $"{Amount:F2}";
}
