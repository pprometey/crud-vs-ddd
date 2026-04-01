using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Schedules.Events;
using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Schedules;

public sealed class ScheduleAgg : AggregateRoot<ScheduleId>
{
    private readonly List<TimeSlot> _slots = [];

    public UserId DoctorId { get; private set; }
    public IReadOnlyList<TimeSlot> Slots => _slots.AsReadOnly();

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

        RegisterEvent(new ScheduleSlotAdded(Id, DoctorId, slotId, start, duration, price.Amount));
        return slot;
    }

    public void RemoveSlot(TimeSlotId slotId)
    {
        var slot = _slots.FirstOrDefault(s => s.Id == slotId)
            ?? throw new SlotNotFoundException(slotId);

        _slots.Remove(slot);
        RegisterEvent(new ScheduleSlotRemoved(Id, slotId));
    }

    public TimeSlot? FindSlot(TimeSlotId slotId) => _slots.FirstOrDefault(s => s.Id == slotId);
}
