using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Configurations;

public sealed class AppointmentDbModelConfiguration : IEntityTypeConfiguration<AppointmentDbModel>
{
    public void Configure(EntityTypeBuilder<AppointmentDbModel> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SlotId)
            .IsRequired();

        builder.Property(a => a.PatientId)
            .IsRequired();

        builder.Property(a => a.DoctorId)
            .IsRequired();

        builder.Property(a => a.SlotStart)
            .IsRequired();

        builder.Property(a => a.SlotPriceAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.HasMany(a => a.Payments)
            .WithOne(p => p.Appointment)
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.DoctorId);
        builder.HasIndex(a => a.SlotId);

        builder.Property(a => a.Version)
            .IsConcurrencyToken();
    }
}
