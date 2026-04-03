using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Tests.Fakes;
using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Domain.Tests.Schedules;

public class ScheduleTests
{
    private static readonly DateTime Now = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly UserId DoctorId = UserId.New();

    private static ScheduleAgg CreateSchedule()
        => new(ScheduleId.New(), DoctorId);

    private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
    private static readonly Money Price = new(100);

    [Fact]
    public void AddSlot_FutureTime_Succeeds()
    {
        var schedule = CreateSchedule();
        var start = Now.AddDays(1);

        var slot = schedule.AddSlot(TimeSlotId.New(), start, OneHour, Price, Now);

        Assert.NotNull(slot);
        Assert.Equal(start, slot.Start);
        Assert.Equal(Price, slot.Price);
        Assert.Equal(DoctorId, slot.DoctorId);
        Assert.Single(schedule.Slots);
    }

    [Fact]
    public void AddSlot_PastTime_ThrowsDomainException()
    {
        var schedule = CreateSchedule();
        var past = Now.AddHours(-1);

        Assert.Throws<DomainException>(() =>
            schedule.AddSlot(TimeSlotId.New(), past, OneHour, Price, Now));
    }

    [Fact]
    public void AddSlot_ExactlyNow_ThrowsDomainException()
    {
        var schedule = CreateSchedule();

        Assert.Throws<DomainException>(() =>
            schedule.AddSlot(TimeSlotId.New(), Now, OneHour, Price, Now));
    }

    [Fact]
    public void AddSlot_OverlappingTime_ThrowsDomainException()
    {
        var schedule = CreateSchedule();
        var start = Now.AddDays(1).AddHours(10);
        schedule.AddSlot(TimeSlotId.New(), start, OneHour, Price, Now);

        // Overlapping slot: starts 30 minutes into the first slot
        var overlapping = start.AddMinutes(30);
        Assert.Throws<DomainException>(() =>
            schedule.AddSlot(TimeSlotId.New(), overlapping, OneHour, Price, Now));
    }

    [Fact]
    public void AddSlot_AdjacentNotOverlapping_Succeeds()
    {
        var schedule = CreateSchedule();
        var start1 = Now.AddDays(1).AddHours(9);
        var start2 = start1 + OneHour; // starts exactly when the first ends

        schedule.AddSlot(TimeSlotId.New(), start1, OneHour, Price, Now);
        schedule.AddSlot(TimeSlotId.New(), start2, OneHour, Price, Now); // should not throw

        Assert.Equal(2, schedule.Slots.Count);
    }

    [Fact]
    public void RemoveSlot_ExistingSlot_RemovesIt()
    {
        var schedule = CreateSchedule();
        var slotId = TimeSlotId.New();
        schedule.AddSlot(slotId, Now.AddDays(1), OneHour, Price, Now);

        schedule.RemoveSlot(slotId);

        Assert.Empty(schedule.Slots);
    }

    [Fact]
    public void RemoveSlot_NonExistentSlot_ThrowsSlotNotFound()
    {
        var schedule = CreateSchedule();

        Assert.Throws<SlotNotFoundException>(() =>
            schedule.RemoveSlot(TimeSlotId.New()));
    }

    [Fact]
    public void FindSlot_ExistingSlot_ReturnsIt()
    {
        var schedule = CreateSchedule();
        var slotId = TimeSlotId.New();
        schedule.AddSlot(slotId, Now.AddDays(1), OneHour, Price, Now);

        var found = schedule.FindSlot(slotId);

        Assert.NotNull(found);
        Assert.Equal(slotId, found!.Id);
    }

    [Fact]
    public void FindSlot_MissingSlot_ReturnsNull()
    {
        var schedule = CreateSchedule();

        Assert.Null(schedule.FindSlot(TimeSlotId.New()));
    }

    [Fact]
    public void SlotCancellationPolicy_ActiveAppointmentExists_Throws()
    {
        var slotId = TimeSlotId.New();
        var patientId = UserId.New();
        var doctorId = UserId.New();
        var slotStart = Now.AddDays(2);

        var appointmentRepo = new FakeAppointmentRepository();
        var appointment = new AppointmentAgg(
            AppointmentId.New(), slotId, patientId, doctorId, slotStart, new Money(100));
        appointmentRepo.Save(appointment);

        var policy = new DoctorBooking.DDD.Domain.Services.SlotCancellationPolicy(appointmentRepo);

        Assert.Throws<DomainException>(() => policy.AssertCanRemove(slotId));
    }

    [Fact]
    public void SlotCancellationPolicy_NoActiveAppointments_DoesNotThrow()
    {
        var slotId = TimeSlotId.New();
        var appointmentRepo = new FakeAppointmentRepository();
        var policy = new DoctorBooking.DDD.Domain.Services.SlotCancellationPolicy(appointmentRepo);

        // Should not throw
        policy.AssertCanRemove(slotId);
    }
}
