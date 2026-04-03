using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        // --- Users ---
        var doctorId1 = Guid.NewGuid();
        var doctorId2 = Guid.NewGuid();
        var patientId1 = Guid.NewGuid();
        var patientId2 = Guid.NewGuid();

        var doctor1 = new UserDbModel
        {
            Id = doctorId1,
            Email = "doctor1@example.com",
            FirstName = "Ivan",
            LastName = "Petrov"
        };

        var doctor2 = new UserDbModel
        {
            Id = doctorId2,
            Email = "doctor2@example.com",
            FirstName = "Maria",
            LastName = "Sidorova"
        };

        var patient1 = new UserDbModel
        {
            Id = patientId1,
            Email = "patient1@example.com",
            FirstName = "Alexei",
            LastName = "Ivanov"
        };

        var patient2 = new UserDbModel
        {
            Id = patientId2,
            Email = "patient2@example.com",
            FirstName = "Elena",
            LastName = "Kuznetsova"
        };

        context.Users.AddRange(doctor1, doctor2, patient1, patient2);

        // --- User Roles ---
        context.UserRoles.AddRange(
            new UserRoleDbModel { Id = Guid.NewGuid(), UserId = doctorId1, Role = (int)UserRole.Doctor },
            new UserRoleDbModel { Id = Guid.NewGuid(), UserId = doctorId2, Role = (int)UserRole.Doctor },
            new UserRoleDbModel { Id = Guid.NewGuid(), UserId = patientId1, Role = (int)UserRole.Patient },
            new UserRoleDbModel { Id = Guid.NewGuid(), UserId = patientId2, Role = (int)UserRole.Patient }
        );

        // --- Schedules and TimeSlots ---
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        var schedule1Id = Guid.NewGuid();
        var schedule2Id = Guid.NewGuid();

        var schedule1 = new ScheduleDbModel
        {
            Id = schedule1Id,
            DoctorId = doctorId1
        };

        var schedule2 = new ScheduleDbModel
        {
            Id = schedule2Id,
            DoctorId = doctorId2
        };

        context.Schedules.AddRange(schedule1, schedule2);

        var slot1Id = Guid.NewGuid();
        var slot2Id = Guid.NewGuid();
        var slot3Id = Guid.NewGuid();
        var slot4Id = Guid.NewGuid();

        var slot1 = new TimeSlotDbModel
        {
            Id = slot1Id,
            ScheduleId = schedule1Id,
            Start = tomorrow.AddHours(9),
            DurationTicks = TimeSpan.FromHours(1).Ticks,
            PriceAmount = 1500m,
            DoctorId = doctorId1
        };

        var slot2 = new TimeSlotDbModel
        {
            Id = slot2Id,
            ScheduleId = schedule1Id,
            Start = tomorrow.AddHours(10),
            DurationTicks = TimeSpan.FromHours(1).Ticks,
            PriceAmount = 1200m,
            DoctorId = doctorId1
        };

        var slot3 = new TimeSlotDbModel
        {
            Id = slot3Id,
            ScheduleId = schedule2Id,
            Start = tomorrow.AddHours(9),
            DurationTicks = TimeSpan.FromHours(1).Ticks,
            PriceAmount = 2000m,
            DoctorId = doctorId2
        };

        var slot4 = new TimeSlotDbModel
        {
            Id = slot4Id,
            ScheduleId = schedule2Id,
            Start = tomorrow.AddHours(10),
            DurationTicks = TimeSpan.FromHours(1).Ticks,
            PriceAmount = 1800m,
            DoctorId = doctorId2
        };

        context.TimeSlots.AddRange(slot1, slot2, slot3, slot4);

        // --- Appointments ---
        var now = DateTime.UtcNow;

        var appt1Id = Guid.NewGuid();
        var appt2Id = Guid.NewGuid();

        var appt1 = new AppointmentDbModel
        {
            Id = appt1Id,
            SlotId = slot1Id,
            PatientId = patientId1,
            DoctorId = doctorId1,
            SlotStart = slot1.Start,
            SlotPriceAmount = slot1.PriceAmount,
            Status = 0 // Scheduled
        };

        var appt2 = new AppointmentDbModel
        {
            Id = appt2Id,
            SlotId = slot3Id,
            PatientId = patientId2,
            DoctorId = doctorId2,
            SlotStart = slot3.Start,
            SlotPriceAmount = slot3.PriceAmount,
            Status = 0 // Scheduled
        };

        context.Appointments.AddRange(appt1, appt2);

        // --- Payments ---
        context.Payments.AddRange(
            new PaymentDbModel
            {
                Id = Guid.NewGuid(),
                AppointmentId = appt1Id,
                Amount = 1500m,
                PaidAt = now,
                Status = 1 // Paid
            },
            new PaymentDbModel
            {
                Id = Guid.NewGuid(),
                AppointmentId = appt2Id,
                Amount = 2000m,
                PaidAt = now,
                Status = 1 // Paid
            }
        );

        context.SaveChanges();
    }
}
