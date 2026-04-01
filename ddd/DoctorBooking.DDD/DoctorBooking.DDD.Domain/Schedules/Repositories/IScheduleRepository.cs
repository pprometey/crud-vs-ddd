using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Schedules;

namespace DoctorBooking.DDD.Domain.Schedules;

public interface IScheduleRepository
{
    ScheduleAgg? FindById(ScheduleId id);
    ScheduleAgg? FindByDoctor(UserId doctorId);

    /// <summary>
    /// Cross-aggregate read: returns the TimeSlot regardless of which ScheduleAgg owns it.
    /// Used by booking and cancellation services.
    /// </summary>
    TimeSlot? FindSlotById(TimeSlotId slotId);

    void Save(ScheduleAgg schedule);
}
