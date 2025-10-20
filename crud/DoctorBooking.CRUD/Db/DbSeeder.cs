namespace DoctorBooking.CRUD.Db;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DbSeeder
{
    public static void Seed(MedicalBookingContext context)
    {
        if (context.Users.Any())
            return; 

        // --- Пользователи ---
        var user1 = new User { Name = "Доктор Докторович 1", Email = "doctor1@example.com", Phone = "+77000000001", Role = UserRole.Doctor };
        var user2 = new User { Name = "Доктор Докторович 2", Email = "doctor2@example.com", Phone = "+77000000002", Role = UserRole.Doctor };
        var user3 = new User { Name = "Пациент Пациентович 1", Email = "patient1@example.com", Phone = "+77000000003", Role = UserRole.Patient };
        var user4 = new User { Name = "Пациент Пациентович 2", Email = "patient2@example.com", Phone = "+77000000004", Role = UserRole.Patient };

        // --- Врачи ---
        var doctor1 = new Doctor { User = user1, Specialization = "Терапевт" };
        var doctor2 = new Doctor { User = user2, Specialization = "Кардиолог" };

        // --- Пациенты ---
        var patient1 = new Patient { User = user3, DateOfBirth = new DateOnly(1990, 1, 1) };
        var patient2 = new Patient { User = user4, DateOfBirth = new DateOnly(1995, 5, 5) };

        // --- Расписание (слоты) ---
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var slot1 = new Schedule { Doctor = doctor1, Date = tomorrow, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), IsAvailable = false }; // зайдет appointment1
        var slot2 = new Schedule { Doctor = doctor1, Date = tomorrow, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsAvailable = true };
        var slot3 = new Schedule { Doctor = doctor2, Date = tomorrow, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), IsAvailable = false }; // зайдет appointment2
        var slot4 = new Schedule { Doctor = doctor2, Date = tomorrow, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsAvailable = true };

        // --- Записи (appointments) ---
        var apptTime1 = DateTime.Today.AddDays(1).Date.AddHours(9); // tomorrow 09:00
        var apptTime2 = DateTime.Today.AddDays(1).Date.AddHours(9); // tomorrow 09:00 

        var now = DateTime.UtcNow;

        var appointment1 = new Appointment
        {
            Doctor = doctor1,
            Patient = patient1,
            Schedule = slot1,
            ScheduledTime = apptTime1,
            Status = AppointmentStatus.Scheduled,
            Payments = new List<Payment>() 
        };

        var appointment2 = new Appointment
        {
            Doctor = doctor2,
            Patient = patient2,
            Schedule = slot3,
            ScheduledTime = apptTime2,
            Status = AppointmentStatus.Scheduled,
            Payments = new List<Payment>()
        };

        // --- Платежи ---
        var payment1 = new Payment
        {
            Appointment = appointment1,
            Amount = 15000m,
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow
        };

        var payment2 = new Payment
        {
            Appointment = appointment2,
            Amount = 20000m,
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow
        };

        // Привязываем платежи к записи
        appointment1.Payments.Add(payment1);
        appointment2.Payments.Add(payment2);

        // Убедимся, что соответствующие слоты помечены как занятые
        slot1.IsAvailable = false;
        slot3.IsAvailable = false;

        // Добавляем всё в контекст одной операцией
        context.Users.AddRange(user1, user2, user3, user4);
        context.Doctors.AddRange(doctor1, doctor2);
        context.Patients.AddRange(patient1, patient2);
        context.Schedules.AddRange(slot1, slot2, slot3, slot4);
        context.Appointments.AddRange(appointment1, appointment2);
        context.Payments.AddRange(payment1, payment2);

        context.SaveChanges();
    }
}

