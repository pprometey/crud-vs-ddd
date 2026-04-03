using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion<PaymentIdConverter>();

        builder.Property(p => p.Amount)
            .HasConversion<MoneyConverter>()
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.PaidAt).IsRequired();

        builder.Property(p => p.Status).IsRequired();

        builder.Property<AppointmentId>("AppointmentId")
            .HasConversion<AppointmentIdConverter>()
            .IsRequired();
    }
}
