using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Users;
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
    public void AddRole_NewRole_AddsRole()
    {
        var user = CreatePatient();

        user.AddRole(UserRole.Doctor);

        Assert.True(user.HasRole(UserRole.Doctor));
    }

    [Fact]
    public void AddRole_AlreadyExists_Idempotent()
    {
        var user = CreatePatient();

        user.AddRole(UserRole.Patient); // already has it

        Assert.Single(user.Roles); // still just one role
    }

    [Fact]
    public void RemoveRole_UserHasMultipleRoles_Succeeds()
    {
        var user = CreatePatient();
        user.AddRole(UserRole.Doctor);

        user.RemoveRole(UserRole.Doctor);

        Assert.False(user.HasRole(UserRole.Doctor));
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
