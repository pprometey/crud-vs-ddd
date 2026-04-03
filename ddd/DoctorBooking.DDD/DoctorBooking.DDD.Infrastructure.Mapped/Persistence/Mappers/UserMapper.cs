using System.Reflection;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;

public static class UserMapper
{
    public static UserDbModel ToDbModel(UserAgg aggregate)
    {
        return new UserDbModel
        {
            Id = aggregate.Id.Value,
            Email = aggregate.Email.Value,
            FirstName = aggregate.Name.FirstName,
            LastName = aggregate.Name.LastName,
            Version = aggregate.Version,
            Roles = aggregate.Roles.Select(ToDbModel).ToList()
        };
    }

    public static UserAgg ToDomain(UserDbModel dbModel)
    {
        var roles = dbModel.Roles.Select(r => (UserRole)r.Role).ToList();

        // Create with first role
        var aggregate = new UserAgg(
            new UserId(dbModel.Id),
            new Email(dbModel.Email),
            new PersonName(dbModel.FirstName, dbModel.LastName),
            roles[0]);

        // Add remaining roles through public method
        foreach (var role in roles.Skip(1))
        {
            aggregate.AddRole(role);
        }

        HydrateVersion(aggregate, dbModel.Version);
        return aggregate;
    }

    private static void HydrateVersion(UserAgg aggregate, int version)
    {
#pragma warning disable S3011 // Accessibility bypass is by design for ORM hydration without public setters
        typeof(AggregateRoot<UserId>)
            .GetProperty(nameof(AggregateRoot<UserId>.Version))!
            .SetValue(aggregate, version);
#pragma warning restore S3011
    }

    private static UserRoleDbModel ToDbModel(UserRole role)
    {
        return new UserRoleDbModel
        {
            Id = Guid.NewGuid(),
            Role = (int)role
        };
    }
}
