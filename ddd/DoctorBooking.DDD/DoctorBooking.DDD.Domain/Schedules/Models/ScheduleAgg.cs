using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Schedules;

public sealed class ScheduleAgg : AggregateRoot<ScheduleId>
{
    private readonly List<TimeSlot> _slots = [];

    public UserId DoctorId { get; private set; }
    public IReadOnlyList<TimeSlot> Slots => _slots.AsReadOnly();

    // EF Core constructor
    private ScheduleAgg() : base(default!)
    {
        DoctorId = default!;
    }

    public ScheduleAgg(ScheduleId id, UserId doctorId) : base(id)
    {
        DoctorId = doctorId;
    }

    public TimeSlot AddSlot(TimeSlotId slotId, DateTime start, TimeSpan duration, Money price, DateTime now)
    {
        Guard.Against.SlotInPast(start, now);
        Guard.Against.OverlappingSlot(_slots, start, duration);

        var slot = new TimeSlot(slotId, start, duration, price, DoctorId);
        _slots.Add(slot);
        return slot;
    }

    public void RemoveSlot(TimeSlotId slotId)
    {
        var slot = _slots.FirstOrDefault(s => s.Id == slotId)
            ?? throw new SlotNotFoundException(slotId);

        _slots.Remove(slot);
    }

    public TimeSlot? FindSlot(TimeSlotId slotId) => _slots.FirstOrDefault(s => s.Id == slotId);
}
