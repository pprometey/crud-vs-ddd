using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Configurations;

public sealed class TimeSlotDbModelConfiguration : IEntityTypeConfiguration<TimeSlotDbModel>
{
    public void Configure(EntityTypeBuilder<TimeSlotDbModel> builder)
    {
        builder.ToTable("TimeSlots");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Start)
            .IsRequired();

        builder.Property(t => t.DurationTicks)
            .IsRequired();

        builder.Property(t => t.PriceAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.DoctorId)
            .IsRequired();

        builder.Property(t => t.ScheduleId)
            .IsRequired();

        builder.HasIndex(t => t.ScheduleId);
    }
}
