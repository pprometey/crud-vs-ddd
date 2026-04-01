using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Users.Events;
using DoctorBooking.DDD.Domain.Errors;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Users;

public sealed class UserAgg : AggregateRoot<UserId>
{
    private readonly HashSet<UserRole> _roles;

    public Email Email { get; private set; }
    public PersonName Name { get; private set; }
    public IReadOnlySet<UserRole> Roles => _roles;

    public UserAgg(UserId id, Email email, PersonName name, UserRole initialRole = UserRole.Patient)
        : base(id)
    {
        Email = email;
        Name = name;
        _roles = [initialRole];

        RegisterEvent(new UserRegistered(id, email, [initialRole]));
    }

    public void AddRole(UserRole role)
    {
        if (_roles.Contains(role)) return; // idempotent
        _roles.Add(role);
        RegisterEvent(new UserRoleAdded(Id, role));
    }

    public void RemoveRole(UserRole role)
    {
        Guard.Against.LastRoleRemoval(_roles, role);

        if (_roles.Remove(role))
            RegisterEvent(new UserRoleRemoved(Id, role));
    }

    public bool HasRole(UserRole role) => _roles.Contains(role);
    public bool IsDoctor() => HasRole(UserRole.Doctor);
    public bool IsPatient() => HasRole(UserRole.Patient);
}
