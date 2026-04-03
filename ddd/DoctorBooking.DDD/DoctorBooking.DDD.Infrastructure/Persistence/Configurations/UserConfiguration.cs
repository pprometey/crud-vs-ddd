using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserAgg>
{
    public void Configure(EntityTypeBuilder<UserAgg> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion<UserIdConverter>();

        builder.Property(u => u.Email)
            .HasConversion<EmailConverter>()
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.ComplexProperty(u => u.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(u => u.Version)
            .IsConcurrencyToken();

        builder.Property<DateTime>("CreatedAt").IsRequired();

        // Roles stored in normalized UserRoles table via UserRoleEntry.
        // Domain's HashSet<UserRole> is synced in repository Save/Load.
        builder.Ignore(u => u.Roles);
    }
}

