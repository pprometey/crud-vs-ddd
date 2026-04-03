using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Tests.Mappers;

public class UserMapperTests
{
    [Fact]
    public void RoundTrip_SingleRole_PreservesAllData()
    {
        // Arrange
        var originalId = UserId.New();
        var original = new UserAgg(
            originalId,
            new Email("doctor@hospital.com"),
            new PersonName("Jane", "Smith"),
            UserRole.Doctor);

        // Act
        var dbModel = UserMapper.ToDbModel(original);
        var restored = UserMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Email, restored.Email);
        Assert.Equal(original.Name, restored.Name);
        Assert.Single(restored.Roles);
        Assert.True(restored.HasRole(UserRole.Doctor));
    }

    [Fact]
    public void RoundTrip_MultipleRoles_PreservesAllRoles()
    {
        // Arrange
        var original = new UserAgg(
            UserId.New(),
            new Email("admin@hospital.com"),
            new PersonName("Admin", "User"),
            UserRole.Patient);
        
        original.AddRole(UserRole.Doctor);

        // Act
        var dbModel = UserMapper.ToDbModel(original);
        var restored = UserMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(2, restored.Roles.Count);
        Assert.True(restored.HasRole(UserRole.Patient));
        Assert.True(restored.HasRole(UserRole.Doctor));
        Assert.True(restored.IsDoctor());
        Assert.True(restored.IsPatient());
    }

    [Fact]
    public void RoundTrip_PatientRole_PreservesData()
    {
        // Arrange
        var original = new UserAgg(
            UserId.New(),
            new Email("patient@example.com"),
            new PersonName("John", "Doe"));

        // Act
        var dbModel = UserMapper.ToDbModel(original);
        var restored = UserMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(original.Id, restored.Id);
        Assert.True(restored.IsPatient());
        Assert.False(restored.IsDoctor());
    }

    [Fact]
    public void ToDbModel_CreatesCorrectDbModel()
    {
        // Arrange
        var userId = UserId.New();
        var aggregate = new UserAgg(
            userId,
            new Email("test@test.com"),
            new PersonName("First", "Last"),
            UserRole.Doctor);

        // Act
        var dbModel = UserMapper.ToDbModel(aggregate);

        // Assert
        Assert.Equal(userId.Value, dbModel.Id);
        Assert.Equal("test@test.com", dbModel.Email);
        Assert.Equal("First", dbModel.FirstName);
        Assert.Equal("Last", dbModel.LastName);
        Assert.Single(dbModel.Roles);
        Assert.Equal((int)UserRole.Doctor, dbModel.Roles[0].Role);
    }

    [Fact]
    public void ToDomain_FromDbModel_CreatesValidAggregate()
    {
        // Arrange
        var dbModel = new Mapped.Persistence.Models.UserDbModel
        {
            Id = Guid.NewGuid(),
            Email = "new@test.com",
            FirstName = "New",
            LastName = "Person",
            Roles = new List<Mapped.Persistence.Models.UserRoleDbModel>
            {
                new() { Id = Guid.NewGuid(), Role = (int)UserRole.Patient }
            }
        };

        // Act
        var aggregate = UserMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(new UserId(dbModel.Id), aggregate.Id);
        Assert.Equal(new Email("new@test.com"), aggregate.Email);
        Assert.Equal(new PersonName("New", "Person"), aggregate.Name);
        Assert.True(aggregate.HasRole(UserRole.Patient));
    }

    [Fact]
    public void RoundTrip_Version_PreservedCorrectly()
    {
        // Arrange
        var dbModel = new UserDbModel
        {
            Id = Guid.NewGuid(),
            Email = "versioned@test.com",
            FirstName = "Ver",
            LastName = "Sion",
            Version = 5,
            Roles = [new UserRoleDbModel { Id = Guid.NewGuid(), Role = (int)UserRole.Patient }]
        };

        // Act
        var aggregate = UserMapper.ToDomain(dbModel);
        var roundTripped = UserMapper.ToDbModel(aggregate);

        // Assert
        Assert.Equal(5, aggregate.Version);
        Assert.Equal(5, roundTripped.Version);
    }
}
