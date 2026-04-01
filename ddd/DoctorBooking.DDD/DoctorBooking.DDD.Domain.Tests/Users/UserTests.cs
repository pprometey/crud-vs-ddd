using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Users.Events;
using Xunit;

namespace DoctorBooking.DDD.Domain.Tests.Users;

public class UserTests
{
    private static UserAgg CreatePatient(UserId? id = null)
    {
        var userId = id ?? UserId.New();
        return new UserAgg(userId, new Email("patient@test.com"), new PersonName("John", "Doe"));
    }

    [Fact]
    public void NewUser_HasPatientRoleByDefault()
    {
        var user = CreatePatient();
        Assert.True(user.HasRole(UserRole.Patient));
        Assert.False(user.HasRole(UserRole.Doctor));
    }

    [Fact]
    public void NewUser_RegistersUserRegisteredEvent()
    {
        var user = CreatePatient();
        var events = user.PopDomainEvents();
        Assert.Single(events);
        Assert.IsType<UserRegistered>(events[0]);
    }

    [Fact]
    public void AddRole_NewRole_AddsAndRegistersEvent()
    {
        var user = CreatePatient();
        user.PopDomainEvents(); // clear initial

        user.AddRole(UserRole.Doctor);

        Assert.True(user.HasRole(UserRole.Doctor));
        var events = user.PopDomainEvents();
        Assert.Single(events);
        var e = Assert.IsType<UserRoleAdded>(events[0]);
        Assert.Equal(UserRole.Doctor, e.Role);
    }

    [Fact]
    public void AddRole_AlreadyExists_Idempotent()
    {
        var user = CreatePatient();
        user.PopDomainEvents();

        user.AddRole(UserRole.Patient); // already has it

        Assert.Empty(user.PopDomainEvents());
        Assert.Single(user.Roles); // still just one role
    }

    [Fact]
    public void RemoveRole_UserHasMultipleRoles_Succeeds()
    {
        var user = CreatePatient();
        user.AddRole(UserRole.Doctor);
        user.PopDomainEvents();

        user.RemoveRole(UserRole.Doctor);

        Assert.False(user.HasRole(UserRole.Doctor));
        var events = user.PopDomainEvents();
        var e = Assert.IsType<UserRoleRemoved>(events[0]);
        Assert.Equal(UserRole.Doctor, e.Role);
    }

    [Fact]
    public void RemoveRole_LastRole_ThrowsDomainException()
    {
        var user = CreatePatient();

        var ex = Assert.Throws<DomainException>(() => user.RemoveRole(UserRole.Patient));
        Assert.Contains("at least one role", ex.Message);
    }

    [Fact]
    public void IsDoctor_ReflectsRole()
    {
        var user = CreatePatient();
        Assert.False(user.IsDoctor());

        user.AddRole(UserRole.Doctor);
        Assert.True(user.IsDoctor());
    }

    [Fact]
    public void IsPatient_ReflectsRole()
    {
        var user = CreatePatient();
        Assert.True(user.IsPatient());

        // Must add another role before removing Patient
        user.AddRole(UserRole.Doctor);
        user.RemoveRole(UserRole.Patient);
        Assert.False(user.IsPatient());
    }

    [Fact]
    public void User_CanHaveBothRoles()
    {
        var user = CreatePatient();
        user.AddRole(UserRole.Doctor);

        Assert.True(user.IsDoctor());
        Assert.True(user.IsPatient());
    }
}
