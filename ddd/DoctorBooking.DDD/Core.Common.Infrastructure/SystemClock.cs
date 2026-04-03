using Core.Common.Domain;

namespace Core.Common.Infrastructure;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
