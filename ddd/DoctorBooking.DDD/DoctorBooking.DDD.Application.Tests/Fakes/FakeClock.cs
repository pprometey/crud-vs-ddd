using Core.Common.Domain;

namespace DoctorBooking.DDD.Application.Tests.Fakes;

public class FakeClock : IClock
{
    private DateTime _utcNow;

    public FakeClock(DateTime utcNow) => _utcNow = utcNow;

    public DateTime UtcNow => _utcNow;

    public void Set(DateTime utcNow) => _utcNow = utcNow;

    public void Advance(TimeSpan duration) => _utcNow += duration;
}
