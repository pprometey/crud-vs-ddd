using Core.Common.Application.Persistence;

namespace DoctorBooking.DDD.Application.Tests.Fakes;

/// <summary>
/// Fake UnitOfWork for testing — just tracks whether SaveChangesAsync was called.
/// </summary>
public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
