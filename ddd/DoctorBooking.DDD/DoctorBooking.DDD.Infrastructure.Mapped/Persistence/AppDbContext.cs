using Core.Common.Application.Persistence;
using Core.Common.Domain;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    // DbSets для DbModels (не для domain models!)
    public DbSet<UserDbModel> Users => Set<UserDbModel>();
    public DbSet<UserRoleDbModel> UserRoles => Set<UserRoleDbModel>();
    public DbSet<ScheduleDbModel> Schedules => Set<ScheduleDbModel>();
    public DbSet<TimeSlotDbModel> TimeSlots => Set<TimeSlotDbModel>();
    public DbSet<AppointmentDbModel> Appointments => Set<AppointmentDbModel>();
    public DbSet<PaymentDbModel> Payments => Set<PaymentDbModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IncrementVersions();
        SetCreatedAt();
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex);
        }
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }

    private void IncrementVersions()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified &&
                entry.Metadata.FindProperty("Version") is not null)
            {
                var version = (int)entry.Property("Version").CurrentValue!;
                entry.Property("Version").CurrentValue = version + 1;
            }
        }
    }

    private void SetCreatedAt()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") is not null)
            {
                entry.Property("CreatedAt").CurrentValue = now;
            }
        }
    }
}
