using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Schedules.Events;

public sealed record ScheduleSlotAdded(
    ScheduleId ScheduleId,
    UserId DoctorId,
    TimeSlotId SlotId,
    DateTime Start,
    TimeSpan Duration,
    decimal Price) : DomainEvent;
