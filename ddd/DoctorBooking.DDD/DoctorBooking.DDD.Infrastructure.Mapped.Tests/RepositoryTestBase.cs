using DoctorBooking.DDD.Infrastructure.Mapped.Tests.Fixtures;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Tests;

/// <summary>
/// Base class for repository integration tests
/// </summary>
public abstract class RepositoryTestBase
{
    protected readonly MappedRepositoryFixture Fixture;

    protected RepositoryTestBase(MappedRepositoryFixture fixture)
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
