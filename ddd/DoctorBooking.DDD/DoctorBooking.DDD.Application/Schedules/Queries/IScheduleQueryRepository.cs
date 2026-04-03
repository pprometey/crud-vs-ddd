using DoctorBooking.DDD.Application.Schedules.Dtos;

namespace DoctorBooking.DDD.Application.Schedules.Queries;

/// <summary>
/// Query repository for read-only Schedule operations (no tracking, direct DTO projection).
/// </summary>
public interface IScheduleQueryRepository
{
    Task<ScheduleDto?> GetByDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<TimeSlotDto?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeSlotDto>> GetAvailableSlotsAsync(
        Guid doctorId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
