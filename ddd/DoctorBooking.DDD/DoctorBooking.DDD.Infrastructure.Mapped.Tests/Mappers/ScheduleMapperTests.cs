using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Tests.Mappers;

public class ScheduleMapperTests
{
    [Fact]
    public void RoundTrip_EmptySchedule_PreservesData()
    {
        // Arrange
        var doctorId = UserId.New();
        var scheduleId = ScheduleId.New();
        var original = new ScheduleAgg(scheduleId, doctorId);

        // Act
        var dbModel = ScheduleMapper.ToDbModel(original);
        var restored = ScheduleMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.DoctorId, restored.DoctorId);
        Assert.Empty(restored.Slots);
    }

    [Fact]
    public void RoundTrip_WithSlots_PreservesAllSlots()
    {
        // Arrange
        var doctorId = UserId.New();
        var original = new ScheduleAgg(ScheduleId.New(), doctorId);
        
        var now = DateTime.UtcNow.Date.AddHours(10);
        var slotTime1 = now.AddDays(1);
        var slotTime2 = now.AddDays(2);
        
        original.AddSlot(TimeSlotId.New(), slotTime1, TimeSpan.FromMinutes(30), new Money(100), now);
        original.AddSlot(TimeSlotId.New(), slotTime2, TimeSpan.FromMinutes(60), new Money(200), now);

        // Act
        var dbModel = ScheduleMapper.ToDbModel(original);
        var restored = ScheduleMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.DoctorId, restored.DoctorId);
        Assert.Equal(2, restored.Slots.Count);
        
        var restoredSlots = restored.Slots.OrderBy(s => s.Start).ToList();
        Assert.Equal(slotTime1, restoredSlots[0].Start);
        Assert.Equal(TimeSpan.FromMinutes(30), restoredSlots[0].Duration);
        Assert.Equal(new Money(100), restoredSlots[0].Price);
        
        Assert.Equal(slotTime2, restoredSlots[1].Start);
        Assert.Equal(TimeSpan.FromMinutes(60), restoredSlots[1].Duration);
        Assert.Equal(new Money(200), restoredSlots[1].Price);
    }

    [Fact]
    public void RoundTrip_SlotIds_PreservedCorrectly()
    {
        // Arrange
        var doctorId = UserId.New();
        var original = new ScheduleAgg(ScheduleId.New(), doctorId);
        
        var slotId = TimeSlotId.New();
        var slotTime = DateTime.UtcNow.Date.AddDays(1).AddHours(14);
        
        original.AddSlot(slotId, slotTime, TimeSpan.FromMinutes(45), new Money(150), DateTime.UtcNow);

        // Act
        var dbModel = ScheduleMapper.ToDbModel(original);
        var restored = ScheduleMapper.ToDomain(dbModel);

        // Assert
        var restoredSlot = restored.Slots.Single();
        Assert.Equal(slotId, restoredSlot.Id);
    }

    [Fact]
    public void ToDbModel_CreatesCorrectStructure()
    {
        // Arrange
        var scheduleId = ScheduleId.New();
        var doctorId = UserId.New();
        var aggregate = new ScheduleAgg(scheduleId, doctorId);
        
        var slotTime = DateTime.UtcNow.Date.AddDays(5);
        aggregate.AddSlot(
            TimeSlotId.New(),
            slotTime,
            TimeSpan.FromMinutes(30),
            new Money(100),
            DateTime.UtcNow);

        // Act
        var dbModel = ScheduleMapper.ToDbModel(aggregate);

        // Assert
        Assert.Equal(scheduleId.Value, dbModel.Id);
        Assert.Equal(doctorId.Value, dbModel.DoctorId);
        Assert.Single(dbModel.Slots);
        
        var slotDb = dbModel.Slots[0];
        Assert.Equal(slotTime, slotDb.Start);
        Assert.Equal(TimeSpan.FromMinutes(30).Ticks, slotDb.DurationTicks);
        Assert.Equal(100, slotDb.PriceAmount);
        Assert.Equal(doctorId.Value, slotDb.DoctorId);
    }

    [Fact]
    public void RoundTrip_MultipleSlotsSameDay_PreservesOrder()
    {
        // Arrange
        var doctorId = UserId.New();
        var original = new ScheduleAgg(ScheduleId.New(), doctorId);
        
        var baseDate = DateTime.UtcNow.Date.AddDays(3);
        var slot1Time = baseDate.AddHours(9);
        var slot2Time = baseDate.AddHours(10);
        var slot3Time = baseDate.AddHours(11);
        
        var now = DateTime.UtcNow;
        original.AddSlot(TimeSlotId.New(), slot1Time, TimeSpan.FromMinutes(30), new Money(100), now);
        original.AddSlot(TimeSlotId.New(), slot2Time, TimeSpan.FromMinutes(30), new Money(100), now);
        original.AddSlot(TimeSlotId.New(), slot3Time, TimeSpan.FromMinutes(30), new Money(100), now);

        // Act
        var dbModel = ScheduleMapper.ToDbModel(original);
        var restored = ScheduleMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(3, restored.Slots.Count);
        var sortedSlots = restored.Slots.OrderBy(s => s.Start).ToList();
        Assert.Equal(slot1Time, sortedSlots[0].Start);
        Assert.Equal(slot2Time, sortedSlots[1].Start);
        Assert.Equal(slot3Time, sortedSlots[2].Start);
    }

    [Fact]
    public void RoundTrip_Version_PreservedCorrectly()
    {
        // Arrange
        var dbModel = new ScheduleDbModel
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            Version = 3,
            Slots = []
        };

        // Act
        var aggregate = ScheduleMapper.ToDomain(dbModel);
        var roundTripped = ScheduleMapper.ToDbModel(aggregate);

        // Assert
        Assert.Equal(3, aggregate.Version);
        Assert.Equal(3, roundTripped.Version);
    }
}
