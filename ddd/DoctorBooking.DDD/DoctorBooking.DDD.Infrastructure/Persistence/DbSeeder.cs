using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Persistence.Configurations;

namespace DoctorBooking.DDD.Infrastructure.Persistence;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        // --- Users ---
        var doctorId1 = UserId.New();
        var doctorId2 = UserId.New();
        var patientId1 = UserId.New();
        var patientId2 = UserId.New();

        var doctor1 = new UserAgg(doctorId1, new Email("doctor1@example.com"),
            new PersonName("Ivan", "Petrov"), UserRole.Doctor);
        var doctor2 = new UserAgg(doctorId2, new Email("doctor2@example.com"),
            new PersonName("Maria", "Sidorova"), UserRole.Doctor);
        var patient1 = new UserAgg(patientId1, new Email("patient1@example.com"),
            new PersonName("Alexei", "Ivanov"), UserRole.Patient);
        var patient2 = new UserAgg(patientId2, new Email("patient2@example.com"),
            new PersonName("Elena", "Kuznetsova"), UserRole.Patient);

        context.Users.AddRange(doctor1, doctor2, patient1, patient2);

        // Sync roles to UserRoles table
        SeedRoles(context, doctor1);
        SeedRoles(context, doctor2);
        SeedRoles(context, patient1);
        SeedRoles(context, patient2);

        // --- Schedules ---
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var now = DateTime.UtcNow;

        var schedule1 = new ScheduleAgg(ScheduleId.New(), doctorId1);
        var slot1 = schedule1.AddSlot(TimeSlotId.New(), tomorrow.AddHours(9), TimeSpan.FromHours(1), new Money(1500m), now);
        _ = schedule1.AddSlot(TimeSlotId.New(), tomorrow.AddHours(10), TimeSpan.FromHours(1), new Money(1200m), now);

        var schedule2 = new ScheduleAgg(ScheduleId.New(), doctorId2);
        var slot3 = schedule2.AddSlot(TimeSlotId.New(), tomorrow.AddHours(9), TimeSpan.FromHours(1), new Money(2000m), now);
        _ = schedule2.AddSlot(TimeSlotId.New(), tomorrow.AddHours(10), TimeSpan.FromHours(1), new Money(1800m), now);

        context.Schedules.AddRange(schedule1, schedule2);

        // --- Appointments ---
        var appt1 = new AppointmentAgg(
            AppointmentId.New(), slot1.Id, patientId1, doctorId1,
            slot1.Start, slot1.Price);

        var appt2 = new AppointmentAgg(
            AppointmentId.New(), slot3.Id, patientId2, doctorId2,
            slot3.Start, slot3.Price);

        // Add payments
        appt1.AddPayment(PaymentId.New(), new Money(1500m), now);
        appt2.AddPayment(PaymentId.New(), new Money(2000m), now);

        context.Appointments.AddRange(appt1, appt2);

        // Pop domain events so they don't accumulate
        doctor1.PopDomainEvents();
        doctor2.PopDomainEvents();
        patient1.PopDomainEvents();
        patient2.PopDomainEvents();
        schedule1.PopDomainEvents();
        schedule2.PopDomainEvents();
        appt1.PopDomainEvents();
        appt2.PopDomainEvents();

        context.SaveChanges();
    }

    private static void SeedRoles(AppDbContext context, UserAgg user)
    {
        foreach (var role in user.Roles)
        {
            context.UserRoles.Add(new UserRoleEntry { UserId = user.Id, Role = role });
        }
    }
}
