namespace DoctorBooking.DDD.Domain.Schedules;

public readonly record struct TimeSlotId(Guid Value)
{
    public static TimeSlotId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
