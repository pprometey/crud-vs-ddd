namespace DoctorBooking.DDD.Domain.Appointments;

public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
