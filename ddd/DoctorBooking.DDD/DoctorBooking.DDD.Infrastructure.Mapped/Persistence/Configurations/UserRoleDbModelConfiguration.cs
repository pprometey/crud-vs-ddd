using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Configurations;

public sealed class UserRoleDbModelConfiguration : IEntityTypeConfiguration<UserRoleDbModel>
{
    public void Configure(EntityTypeBuilder<UserRoleDbModel> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Role)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.HasIndex(r => new { r.UserId, r.Role })
            .IsUnique();
    }
}
