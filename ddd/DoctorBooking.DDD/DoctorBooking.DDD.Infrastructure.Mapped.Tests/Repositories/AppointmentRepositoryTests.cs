using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Tests.Fixtures;
using Xunit;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Tests.Repositories;

[Collection("Repository Tests")]
public class AppointmentRepositoryTests : RepositoryTestBase
{
    public AppointmentRepositoryTests(MappedRepositoryFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Save_LoadsFullAggregate_WithAllPayments()
    {
        // Arrange
        var patientId = UserId.New();
        var doctorId = UserId.New();
        var slotId = TimeSlotId.New();
        var slotStart = DateTime.UtcNow.Date.AddDays(5);

        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            slotId,
            patientId,
            doctorId,
            slotStart,
            new Money(200));

        appointment.AddPayment(PaymentId.New(), new Money(100), DateTime.UtcNow);
        appointment.AddPayment(PaymentId.New(), new Money(100), DateTime.UtcNow.AddMinutes(5));

        // Act - Save
        Fixture.AppointmentRepository.Save(appointment);
        await SaveAsync();

        var appointmentId = appointment.Id;
        ClearTracker(); // Detach to force fresh load

        // Act - Load
        var loaded = Fixture.AppointmentRepository.FindById(appointmentId);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(appointmentId, loaded.Id);
        Assert.Equal(patientId, loaded.PatientId);
        Assert.Equal(doctorId, loaded.DoctorId);
        Assert.Equal(slotId, loaded.SlotId);
        Assert.Equal(slotStart, loaded.SlotStart);
        Assert.Equal(new Money(200), loaded.SlotPrice);
        Assert.Equal(AppointmentStatus.Confirmed, loaded.Status);
        
        // Critical: verify nested collection loaded
        Assert.Equal(2, loaded.Payments.Count);
        Assert.Equal(new Money(200), loaded.PaidTotal());
    }

    [Fact]
    public async Task Save_EmptyPayments_LoadsCorrectly()
    {
        // Arrange
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(3),
            new Money(150));

        // Act
        Fixture.AppointmentRepository.Save(appointment);
        await SaveAsync();
        
        ClearTracker();
        var loaded = Fixture.AppointmentRepository.FindById(appointment.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Empty(loaded.Payments);
        Assert.Equal(AppointmentStatus.Planned, loaded.Status);
    }

    [Fact]
    public async Task Save_ModifiesExistingAggregate()
    {
        // Arrange
        var appointment = new AppointmentAgg(
            AppointmentId.New(),
            TimeSlotId.New(),
            UserId.New(),
            UserId.New(),
            DateTime.UtcNow.Date.AddDays(7),
            new Money(300));

        Fixture.AppointmentRepository.Save(appointment);
        await SaveAsync();
        
        ClearTracker();

        // Act - Load and modify
        var loaded = Fixture.AppointmentRepository.FindById(appointment.Id);
        Assert.NotNull(loaded);
        
        loaded.AddPayment(PaymentId.New(), new Money(300), DateTime.UtcNow);
        
        Fixture.AppointmentRepository.Save(loaded);
        await SaveAsync();
        
        ClearTracker();

        // Assert
        var reloaded = Fixture.AppointmentRepository.FindById(appointment.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(AppointmentStatus.Confirmed, reloaded.Status);
        Assert.Single(reloaded.Payments);
    }

    [Fact]
    public void FindById_NonExistent_ReturnsNull()
    {
        // Act
        var result = Fixture.AppointmentRepository.FindById(AppointmentId.New());

        // Assert
        Assert.Null(result);
    }
}
