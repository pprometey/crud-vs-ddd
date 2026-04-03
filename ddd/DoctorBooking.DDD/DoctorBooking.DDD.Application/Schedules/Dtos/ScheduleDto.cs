namespace DoctorBooking.DDD.Application.Schedules.Dtos;

public sealed record ScheduleDto(
    Guid Id,
    Guid DoctorId,
    IReadOnlyList<TimeSlotDto> Slots);

public sealed record TimeSlotDto(
    Guid Id,
    DateTime Start,
    TimeSpan Duration,
    DateTime End,
    decimal Price,
    Guid DoctorId);
