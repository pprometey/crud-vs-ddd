using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Configurations;

public sealed class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.ToTable("TimeSlots");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion<TimeSlotIdConverter>();

        builder.Property(s => s.Start).IsRequired();

        builder.Property(s => s.Duration)
            .HasConversion<TimeSpanToTicksConverter>()
            .HasColumnName("DurationTicks")
            .IsRequired();

        builder.Property(s => s.Price)
            .HasConversion<MoneyConverter>()
            .HasColumnName("PriceAmount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.DoctorId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.Property<ScheduleId>("ScheduleId")
            .HasConversion<ScheduleIdConverter>()
            .IsRequired();
        builder.HasIndex("ScheduleId");

        builder.Ignore(s => s.End);
    }
}
