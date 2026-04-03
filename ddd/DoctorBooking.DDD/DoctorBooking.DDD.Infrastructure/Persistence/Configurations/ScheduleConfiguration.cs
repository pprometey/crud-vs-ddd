using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Configurations;

public sealed class ScheduleConfiguration : IEntityTypeConfiguration<ScheduleAgg>
{
    public void Configure(EntityTypeBuilder<ScheduleAgg> builder)
    {
        builder.ToTable("Schedules");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion<ScheduleIdConverter>();

        builder.Property(s => s.DoctorId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.HasIndex(s => s.DoctorId).IsUnique();

        builder.HasMany(s => s.Slots)
            .WithOne()
            .HasForeignKey("ScheduleId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Slots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(s => s.Version)
            .IsConcurrencyToken();

        builder.Property<DateTime>("CreatedAt").IsRequired();
    }
}
