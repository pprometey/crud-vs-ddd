namespace DoctorBooking.DDD.Domain.Schedules;

public readonly record struct ScheduleId(Guid Value)
{
    public static ScheduleId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
