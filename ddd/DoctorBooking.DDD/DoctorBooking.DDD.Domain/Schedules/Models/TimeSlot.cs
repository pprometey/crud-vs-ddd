using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Schedules;

public sealed class TimeSlot : Entity<TimeSlotId>
{
    public DateTime Start { get; private set; }
    public TimeSpan Duration { get; private set; }
    public Money Price { get; private set; }
    public UserId DoctorId { get; private set; }

    public DateTime End => Start + Duration;

    public TimeSlot(TimeSlotId id, DateTime start, TimeSpan duration, Money price, UserId doctorId)
        : base(id)
    {
        Start = start;
        Duration = duration;
        Price = price;
        DoctorId = doctorId;
    }

    public bool OverlapsWith(DateTime otherStart, TimeSpan otherDuration)
    {
        var otherEnd = otherStart + otherDuration;
        return Start < otherEnd && otherStart < End;
    }

    public bool IsFuture(DateTime now) => Start > now;
}
