using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Configurations;

/// <summary>
/// Infrastructure-only entity for normalized storage of user roles.
/// Maps HashSet&lt;UserRole&gt; from domain to a separate "UserRoles" table.
/// </summary>
public sealed class UserRoleEntry
{
    public UserId UserId { get; set; }
    public UserRole Role { get; set; }
}

public sealed class UserRoleEntryConfiguration : IEntityTypeConfiguration<UserRoleEntry>
{
    public void Configure(EntityTypeBuilder<UserRoleEntry> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(r => new { r.UserId, r.Role });

        builder.Property(r => r.UserId)
            .HasConversion<UserIdConverter>();

        builder.Property(r => r.Role).IsRequired();
    }
}
