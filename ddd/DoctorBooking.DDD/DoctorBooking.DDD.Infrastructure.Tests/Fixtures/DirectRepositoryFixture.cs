using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Persistence;
using DoctorBooking.DDD.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Tests.Fixtures;

/// <summary>
/// Fixture for Direct persistence approach (domain models as EF entities)
/// </summary>
public sealed class DirectRepositoryFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;

    public IUserRepository UserRepository { get; }
    public IScheduleRepository ScheduleRepository { get; }
    public IAppointmentRepository AppointmentRepository { get; }

    public DirectRepositoryFixture()
    {
        // In-memory SQLite connection (must stay open)
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        // Initialize repositories
        UserRepository = new EfUserRepository(_context);
        ScheduleRepository = new EfScheduleRepository(_context);
        AppointmentRepository = new EfAppointmentRepository(_context);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}

[CollectionDefinition("Repository Tests")]
public class RepositoryCollection : ICollectionFixture<DirectRepositoryFixture>
{
}
