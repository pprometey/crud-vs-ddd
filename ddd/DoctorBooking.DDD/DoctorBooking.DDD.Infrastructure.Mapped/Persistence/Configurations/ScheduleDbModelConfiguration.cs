using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Configurations;

public sealed class ScheduleDbModelConfiguration : IEntityTypeConfiguration<ScheduleDbModel>
{
    public void Configure(EntityTypeBuilder<ScheduleDbModel> builder)
    {
        builder.ToTable("Schedules");
        
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DoctorId)
            .IsRequired();

        builder.HasMany(s => s.Slots)
            .WithOne(t => t.Schedule)
            .HasForeignKey(t => t.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Version)
            .IsConcurrencyToken();
    }
}
