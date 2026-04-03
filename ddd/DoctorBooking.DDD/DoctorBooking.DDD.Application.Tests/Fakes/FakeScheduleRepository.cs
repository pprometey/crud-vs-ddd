using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Application.Tests.Fakes;

public class FakeScheduleRepository : IScheduleRepository
{
    private readonly Dictionary<ScheduleId, ScheduleAgg> _store = [];

    public ScheduleAgg? FindById(ScheduleId id)
        => _store.TryGetValue(id, out var s) ? s : null;

    public ScheduleAgg? FindByDoctor(UserId doctorId)
        => _store.Values.FirstOrDefault(s => s.DoctorId == doctorId);

    public TimeSlot? FindSlotById(TimeSlotId slotId)
        => _store.Values
            .SelectMany(s => s.Slots)
            .FirstOrDefault(sl => sl.Id == slotId);

    public void Save(ScheduleAgg schedule) => _store[schedule.Id] = schedule;
}
