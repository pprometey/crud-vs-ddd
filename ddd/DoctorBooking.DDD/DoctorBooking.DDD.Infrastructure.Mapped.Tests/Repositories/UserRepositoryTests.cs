using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Tests.Fixtures;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Tests.Repositories;

[Collection("Repository Tests")]
public class UserRepositoryTests : RepositoryTestBase
{
    public UserRepositoryTests(MappedRepositoryFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Save_NewAggregate_HasVersionZero()
    {
        // Arrange
        var user = new UserAgg(
            UserId.New(),
            new Email($"v0-{Guid.NewGuid():N}@test.com"),
            new PersonName("Zero", "Version"));

        // Act
        Fixture.UserRepository.Save(user);
        await SaveAsync();
        ClearTracker();

        var loaded = Fixture.UserRepository.FindById(user.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(0, loaded.Version); // new aggregate starts at 0
    }

    [Fact]
    public async Task Save_ModifiedAggregate_IncrementsVersion()
    {
        // Arrange
        var user = new UserAgg(
            UserId.New(),
            new Email($"vinc-{Guid.NewGuid():N}@test.com"),
            new PersonName("Inc", "Version"));

        Fixture.UserRepository.Save(user);
        await SaveAsync();
        ClearTracker();

        // Act - Load, modify, save
        var loaded = Fixture.UserRepository.FindById(user.Id)!;
        loaded.AddRole(UserRole.Doctor);
        Fixture.UserRepository.Save(loaded);
        await SaveAsync();
        ClearTracker();

        // Assert
        var reloaded = Fixture.UserRepository.FindById(user.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(1, reloaded.Version); // incremented from 0 to 1 on update
    }

    [Fact]
    public async Task Save_LoadsFullAggregate_WithAllRoles()
    {
        // Arrange
        var user = new UserAgg(
            UserId.New(),
            new Email("doctor@test.com"),
            new PersonName("Jane", "Smith"),
            UserRole.Doctor);

        user.AddRole(UserRole.Patient);

        // Act - Save
        Fixture.UserRepository.Save(user);
        await SaveAsync();

        var userId = user.Id;
        ClearTracker();

        // Act - Load
        var loaded = Fixture.UserRepository.FindById(userId);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(userId, loaded.Id);
        Assert.Equal(new Email("doctor@test.com"), loaded.Email);
        Assert.Equal(new PersonName("Jane", "Smith"), loaded.Name);
        
        // Critical: verify nested collection loaded
        Assert.Equal(2, loaded.Roles.Count);
        Assert.True(loaded.HasRole(UserRole.Doctor));
        Assert.True(loaded.HasRole(UserRole.Patient));
    }

    [Fact]
    public async Task Save_SingleRole_LoadsCorrectly()
    {
        // Arrange
        var user = new UserAgg(
            UserId.New(),
            new Email("patient@test.com"),
            new PersonName("John", "Doe"));

        // Act
        Fixture.UserRepository.Save(user);
        await SaveAsync();
        
        ClearTracker();
        var loaded = Fixture.UserRepository.FindById(user.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Single(loaded.Roles);
        Assert.True(loaded.IsPatient());
        Assert.False(loaded.IsDoctor());
    }

    [Fact]
    public async Task Save_ModifiesRoles()
    {
        // Arrange
        var user = new UserAgg(
            UserId.New(),
            new Email("user@test.com"),
            new PersonName("Test", "User"));

        Fixture.UserRepository.Save(user);
        await SaveAsync();
        
        ClearTracker();

        // Act - Load and add role
        var loaded = Fixture.UserRepository.FindById(user.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.Roles);
        
        loaded.AddRole(UserRole.Doctor);
        
        Fixture.UserRepository.Save(loaded);
        await SaveAsync();
        
        ClearTracker();

        // Assert
        var reloaded = Fixture.UserRepository.FindById(user.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(2, reloaded.Roles.Count);
        Assert.True(reloaded.IsDoctor());
    }

    [Fact]
    public async Task FindByEmail_FindsUser()
    {
        // Arrange
        var email = new Email("findme@test.com");
        var user = new UserAgg(
            UserId.New(),
            email,
            new PersonName("Find", "Me"),
            UserRole.Patient);

        Fixture.UserRepository.Save(user);
        await SaveAsync();
        
        ClearTracker();

        // Act
        var loaded = Fixture.UserRepository.FindByEmail(email);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(user.Id, loaded.Id);
        Assert.Equal(email, loaded.Email);
        Assert.Single(loaded.Roles);
    }

    [Fact]
    public void FindByEmail_NonExistent_ReturnsNull()
    {
        // Act
        var result = Fixture.UserRepository.FindByEmail(new Email("nonexistent@test.com"));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindById_NonExistent_ReturnsNull()
    {
        // Act
        var result = Fixture.UserRepository.FindById(UserId.New());

        // Assert
        Assert.Null(result);
    }
}
