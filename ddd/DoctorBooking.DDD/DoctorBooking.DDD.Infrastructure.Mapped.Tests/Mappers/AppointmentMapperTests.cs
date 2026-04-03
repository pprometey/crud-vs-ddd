using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Tests.Mappers;

public class AppointmentMapperTests
{
    [Fact]
    public void RoundTrip_PlannedAppointment_NoPayments_PreservesData()
    {
        // Arrange
        var patientId = UserId.New();
        var doctorId = UserId.New();
        var slotId = TimeSlotId.New();
        var slotStart = DateTime.UtcNow.Date.AddDays(5).AddHours(14);
        
        var original = new AppointmentAgg(
            AppointmentId.New(),
            slotId,
            patientId,
            doctorId,
            slotStart,
            new Money(150));

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.SlotId, restored.SlotId);
        Assert.Equal(original.PatientId, restored.PatientId);
        Assert.Equal(original.DoctorId, restored.DoctorId);
        Assert.Equal(original.SlotStart, restored.SlotStart);
        Assert.Equal(original.SlotPrice, restored.SlotPrice);
        Assert.Equal(AppointmentStatus.Planned, restored.Status);
        Assert.Empty(restored.Payments);
    }

    [Fact]
    public void RoundTrip_ConfirmedAppointment_WithFullPayment_PreservesData()
    {
        // Arrange
        var original = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(3),
            new Money(200));

        var paymentId = PaymentId.New();
        var paidAt = DateTime.UtcNow.AddMinutes(-10);
        original.AddPayment(paymentId, new Money(200), paidAt);

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(AppointmentStatus.Confirmed, restored.Status);
        Assert.Single(restored.Payments);
        Assert.Equal(paymentId, restored.Payments[0].Id);
        Assert.Equal(new Money(200), restored.Payments[0].Amount);
        Assert.Equal(paidAt, restored.Payments[0].PaidAt);
        Assert.Equal(new Money(200), restored.PaidTotal());
        Assert.Equal(Money.Zero, restored.RemainingBalance());
    }

    [Fact]
    public void RoundTrip_FreeAppointment_Confirmed_PreservesData()
    {
        // Arrange
        var original = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(2),
            Money.Zero);

        original.ConfirmFree();

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(AppointmentStatus.Confirmed, restored.Status);
        Assert.Equal(Money.Zero, restored.SlotPrice);
        Assert.Empty(restored.Payments);
    }

    [Fact]
    public void RoundTrip_CompletedAppointment_PreservesStatus()
    {
        // Arrange
        var original = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(-1),
            new Money(100));

        original.AddPayment(PaymentId.New(), new Money(100), DateTime.UtcNow.AddDays(-2));
        original.Complete();

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(AppointmentStatus.Completed, restored.Status);
        Assert.Single(restored.Payments);
    }

    [Fact]
    public void RoundTrip_CancelledAppointment_PreservesStatus()
    {
        // Arrange
        var patientId = UserId.New();
        var slotStart = DateTime.UtcNow.Date.AddDays(10);
        
        var original = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            patientId,
            UserId.New(),
            slotStart,
            new Money(150));

        original.Cancel(patientId, slotStart.AddDays(-5));

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, restored.Status);
    }

    [Fact]
    public void RoundTrip_PartialPayment_PreservesAllPayments()
    {
        // Arrange
        var original = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(4),
            new Money(300));

        var payment1Id = PaymentId.New();
        var payment2Id = PaymentId.New();
        var paidAt1 = DateTime.UtcNow.AddHours(-2);
        var paidAt2 = DateTime.UtcNow.AddHours(-1);

        original.AddPayment(payment1Id, new Money(100), paidAt1);
        original.AddPayment(payment2Id, new Money(50), paidAt2);

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(AppointmentStatus.Planned, restored.Status); // Not fully paid
        Assert.Equal(2, restored.Payments.Count);
        Assert.Equal(new Money(150), restored.PaidTotal());
        Assert.Equal(new Money(150), restored.RemainingBalance());
        
        Assert.Contains(restored.Payments, p => p.Id == payment1Id && p.Amount == new Money(100));
        Assert.Contains(restored.Payments, p => p.Id == payment2Id && p.Amount == new Money(50));
    }

    [Fact]
    public void RoundTrip_MultiplePayments_AutoConfirmed_PreservesData()
    {
        // Arrange
        var original = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(6),
            new Money(250));

        original.AddPayment(PaymentId.New(), new Money(100), DateTime.UtcNow.AddHours(-3));
        original.AddPayment(PaymentId.New(), new Money(100), DateTime.UtcNow.AddHours(-2));
        original.AddPayment(PaymentId.New(), new Money(50), DateTime.UtcNow.AddHours(-1));

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        Assert.Equal(AppointmentStatus.Confirmed, restored.Status);
        Assert.Equal(3, restored.Payments.Count);
        Assert.Equal(new Money(250), restored.PaidTotal());
        Assert.Equal(Money.Zero, restored.RemainingBalance());
    }

    [Fact]
    public void ToDbModel_CreatesCorrectStructure()
    {
        // Arrange
        var appointmentId = AppointmentId.New();
        var slotId = TimeSlotId.New();
        var patientId = UserId.New();
        var doctorId = UserId.New();
        var slotStart = DateTime.UtcNow.Date.AddDays(7).AddHours(10);
        
        var aggregate = new AppointmentAgg(
            appointmentId,
            slotId,
            patientId,
            doctorId,
            slotStart,
            new Money(175));

        var paymentId = PaymentId.New();
        aggregate.AddPayment(paymentId, new Money(175), DateTime.UtcNow);

        // Act
        var dbModel = AppointmentMapper.ToDbModel(aggregate);

        // Assert
        Assert.Equal(appointmentId.Value, dbModel.Id);
        Assert.Equal(slotId.Value, dbModel.SlotId);
        Assert.Equal(patientId.Value, dbModel.PatientId);
        Assert.Equal(doctorId.Value, dbModel.DoctorId);
        Assert.Equal(slotStart, dbModel.SlotStart);
        Assert.Equal(175, dbModel.SlotPriceAmount);
        Assert.Equal((int)AppointmentStatus.Confirmed, dbModel.Status);
        Assert.Single(dbModel.Payments);
        Assert.Equal(paymentId.Value, dbModel.Payments[0].Id);
    }

    [Fact]
    public void RoundTrip_PaymentOrder_PreservedAcrossMappings()
    {
        // Arrange
        var original = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(8),
            new Money(200));

        var time1 = DateTime.UtcNow.AddHours(-5);
        var time2 = DateTime.UtcNow.AddHours(-3);
        var time3 = DateTime.UtcNow.AddHours(-1);

        original.AddPayment(PaymentId.New(), new Money(50), time1);
        original.AddPayment(PaymentId.New(), new Money(100), time2);
        original.AddPayment(PaymentId.New(), new Money(50), time3);

        // Act
        var dbModel = AppointmentMapper.ToDbModel(original);
        var restored = AppointmentMapper.ToDomain(dbModel);

        // Assert
        var restoredPayments = restored.Payments.ToList();
        Assert.Equal(3, restoredPayments.Count);
        Assert.Equal(time1, restoredPayments[0].PaidAt);
        Assert.Equal(time2, restoredPayments[1].PaidAt);
        Assert.Equal(time3, restoredPayments[2].PaidAt);
    }

    [Fact]
    public void RoundTrip_Version_PreservedCorrectly()
    {
        // Arrange
        var dbModel = new AppointmentDbModel
        {
            Id = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            SlotStart = DateTime.UtcNow.Date.AddDays(5),
            SlotPriceAmount = 100,
            Status = (int)AppointmentStatus.Planned,
            Version = 7,
            Payments = []
        };

        // Act
        var aggregate = AppointmentMapper.ToDomain(dbModel);
        var roundTripped = AppointmentMapper.ToDbModel(aggregate);

        // Assert
        Assert.Equal(7, aggregate.Version);
        Assert.Equal(7, roundTripped.Version);
    }
}
