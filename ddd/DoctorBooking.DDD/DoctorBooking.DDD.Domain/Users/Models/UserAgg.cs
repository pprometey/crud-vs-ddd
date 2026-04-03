using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Errors;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Users;

public sealed class UserAgg : AggregateRoot<UserId>
{
    private readonly HashSet<UserRole> _roles;

    public Email Email { get; private set; }
    public PersonName Name { get; private set; }
    public IReadOnlySet<UserRole> Roles => _roles;

    // EF Core constructor
    private UserAgg() : base(default!)
    {
        Email = default!;
        Name = default!;
        _roles = [];
    }

    public UserAgg(UserId id, Email email, PersonName name, UserRole initialRole = UserRole.Patient)
        : base(id)
    {
        Email = email;
        Name = name;
        _roles = [initialRole];
    }

    public void AddRole(UserRole role)
    {
        if (_roles.Contains(role)) return; // idempotent
        _roles.Add(role);
    }

    public void RemoveRole(UserRole role)
    {
        Guard.Against.LastRoleRemoval(_roles, role);
        _roles.Remove(role);
    }

    public bool HasRole(UserRole role) => _roles.Contains(role);
    public bool IsDoctor() => HasRole(UserRole.Doctor);
    public bool IsPatient() => HasRole(UserRole.Patient);
}
