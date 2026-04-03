using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<AppointmentAgg>
{
    public void Configure(EntityTypeBuilder<AppointmentAgg> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion<AppointmentIdConverter>();

        builder.Property(a => a.SlotId)
            .HasConversion<TimeSlotIdConverter>()
            .IsRequired();

        builder.Property(a => a.PatientId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.Property(a => a.DoctorId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.Property(a => a.SlotStart).IsRequired();

        builder.Property(a => a.SlotPrice)
            .HasConversion<MoneyConverter>()
            .HasColumnName("SlotPriceAmount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(a => a.Status).IsRequired();

        builder.HasMany(a => a.Payments)
            .WithOne()
            .HasForeignKey("AppointmentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(a => a.Version)
            .IsConcurrencyToken();

        builder.Property<DateTime>("CreatedAt").IsRequired();

        builder.HasIndex(a => new { a.SlotId, a.Status });
        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.DoctorId);
    }
}
