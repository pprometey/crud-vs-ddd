using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Errors;

namespace DoctorBooking.DDD.Domain.Users;

public readonly record struct Email
{
    public string Value { get; }

    public Email(string value)
    {
        Guard.Against.InvalidEmail(value);
        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}
