using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Tests.Fixtures;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Tests.Repositories;

[Collection("Repository Tests")]
public class ScheduleRepositoryTests : RepositoryTestBase
{
    public ScheduleRepositoryTests(MappedRepositoryFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Save_LoadsFullAggregate_WithAllSlots()
    {
        // Arrange
        var doctorId = UserId.New();
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);

        var now = DateTime.UtcNow;
        var slot1Time = now.AddDays(1);
        var slot2Time = now.AddDays(2);

        schedule.AddSlot(TimeSlotId.New(), slot1Time, TimeSpan.FromMinutes(30), new Money(100), now);
        schedule.AddSlot(TimeSlotId.New(), slot2Time, TimeSpan.FromMinutes(60), new Money(200), now);

        // Act - Save
        Fixture.ScheduleRepository.Save(schedule);
        await SaveAsync();

        var scheduleId = schedule.Id;
        ClearTracker();

        // Act - Load
        var loaded = Fixture.ScheduleRepository.FindById(scheduleId);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(scheduleId, loaded.Id);
        Assert.Equal(doctorId, loaded.DoctorId);
        
        // Critical: verify nested collection loaded
        Assert.Equal(2, loaded.Slots.Count);
        
        var slots = loaded.Slots.OrderBy(s => s.Start).ToList();
        Assert.Equal(slot1Time, slots[0].Start);
        Assert.Equal(TimeSpan.FromMinutes(30), slots[0].Duration);
        Assert.Equal(new Money(100), slots[0].Price);
        
        Assert.Equal(slot2Time, slots[1].Start);
        Assert.Equal(TimeSpan.FromMinutes(60), slots[1].Duration);
        Assert.Equal(new Money(200), slots[1].Price);
    }

    [Fact]
    public async Task Save_EmptySchedule_LoadsCorrectly()
    {
        // Arrange
        var doctorId = UserId.New();
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);

        // Act
        Fixture.ScheduleRepository.Save(schedule);
        await SaveAsync();
        
        ClearTracker();
        var loaded = Fixture.ScheduleRepository.FindById(schedule.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(doctorId, loaded.DoctorId);
        Assert.Empty(loaded.Slots);
    }

    [Fact]
    public async Task Save_AddsNewSlots()
    {
        // Arrange
        var schedule = new ScheduleAgg(ScheduleId.New(), UserId.New());
        
        Fixture.ScheduleRepository.Save(schedule);
        await SaveAsync();
        
        ClearTracker();

        // Act - Load and add slot
        var loaded = Fixture.ScheduleRepository.FindById(schedule.Id);
        Assert.NotNull(loaded);
        Assert.Empty(loaded.Slots);
        
        var slotTime = DateTime.UtcNow.AddDays(5);
        loaded.AddSlot(TimeSlotId.New(), slotTime, TimeSpan.FromMinutes(45), new Money(150), DateTime.UtcNow);
        
        Fixture.ScheduleRepository.Save(loaded);
        await SaveAsync();
        
        ClearTracker();

        // Assert
        var reloaded = Fixture.ScheduleRepository.FindById(schedule.Id);
        Assert.NotNull(reloaded);
        Assert.Single(reloaded.Slots);
        Assert.Equal(slotTime, reloaded.Slots[0].Start);
    }

    [Fact]
    public async Task FindByDoctor_FindsSchedule()
    {
        // Arrange
        var doctorId = UserId.New();
        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        
        schedule.AddSlot(
            TimeSlotId.New(),
            DateTime.UtcNow.AddDays(3),
            TimeSpan.FromMinutes(30),
            new Money(100),
            DateTime.UtcNow);

        Fixture.ScheduleRepository.Save(schedule);
        await SaveAsync();
        
        ClearTracker();

        // Act
        var loaded = Fixture.ScheduleRepository.FindByDoctor(doctorId);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(schedule.Id, loaded.Id);
        Assert.Equal(doctorId, loaded.DoctorId);
        Assert.Single(loaded.Slots);
    }

    [Fact]
    public void FindByDoctor_NonExistent_ReturnsNull()
    {
        // Act
        var result = Fixture.ScheduleRepository.FindByDoctor(UserId.New());

        // Assert
        Assert.Null(result);
    }
}
