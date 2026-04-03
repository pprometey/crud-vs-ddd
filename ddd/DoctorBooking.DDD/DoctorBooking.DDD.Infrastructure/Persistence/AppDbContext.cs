using Core.Common.Application.Persistence;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<UserAgg> Users => Set<UserAgg>();
    public DbSet<UserRoleEntry> UserRoles => Set<UserRoleEntry>();
    public DbSet<ScheduleAgg> Schedules => Set<ScheduleAgg>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<AppointmentAgg> Appointments => Set<AppointmentAgg>();
    public DbSet<Payment> Payments => Set<Payment>();

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
