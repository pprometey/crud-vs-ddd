using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Configurations;

public sealed class PaymentDbModelConfiguration : IEntityTypeConfiguration<PaymentDbModel>
{
    public void Configure(EntityTypeBuilder<PaymentDbModel> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.PaidAt)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.AppointmentId)
            .IsRequired();

        builder.HasIndex(p => p.AppointmentId);
    }
}
