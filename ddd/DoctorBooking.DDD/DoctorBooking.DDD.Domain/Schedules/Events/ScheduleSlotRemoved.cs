using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Schedules.Events;

public sealed record ScheduleSlotRemoved(ScheduleId ScheduleId, TimeSlotId SlotId) : DomainEvent;
