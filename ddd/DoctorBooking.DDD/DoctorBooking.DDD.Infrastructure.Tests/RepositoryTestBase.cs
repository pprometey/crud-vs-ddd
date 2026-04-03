using DoctorBooking.DDD.Infrastructure.Tests.Fixtures;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Tests;

/// <summary>
/// Base class for repository integration tests
/// </summary>
public abstract class RepositoryTestBase
{
    protected readonly DirectRepositoryFixture Fixture;

    protected RepositoryTestBase(DirectRepositoryFixture fixture)
    {
        Fixture = fixture;
    }

    protected async Task SaveAsync()
    {
        await Fixture.SaveChangesAsync();
    }

    protected void ClearTracker()
    {
        Fixture.ClearChangeTracker();
    }
}
