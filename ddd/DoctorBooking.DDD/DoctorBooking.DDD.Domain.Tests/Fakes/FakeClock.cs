using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Tests.Fakes;

public class FakeClock : IClock
{
    public FakeClock(DateTime utcNow) => UtcNow = utcNow;
    public DateTime UtcNow { get; set; }
}
