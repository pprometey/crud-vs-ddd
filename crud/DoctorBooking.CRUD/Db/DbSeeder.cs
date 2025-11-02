namespace DoctorBooking.CRUD.Db;

public static class DbSeeder
{
    public static void Seed(MedicalBookingContext context)
    {
        if (context.Users.Any())
            return;

        // --- Users ---
        var user1 = new User { Name = "Doctor 1", Email = "doctor1@example.com", Phone = "+77000000001" };
        var user2 = new User { Name = "Doctor 2", Email = "doctor2@example.com", Phone = "+77000000002" };
        var user3 = new User { Name = "Patient 1", Email = "patient1@example.com", Phone = "+77000000003" };
        var user4 = new User { Name = "Patient 2", Email = "patient2@example.com", Phone = "+77000000004" };

        // --- Doctors ---
        var doctor1 = new Doctor { User = user1, Specialization = "Therapist" };
        var doctor2 = new Doctor { User = user2, Specialization = "Cardiologist" };

        // --- Patients ---
        var patient1 = new Patient { User = user3, DateOfBirth = new DateOnly(1990, 1, 1) };
        var patient2 = new Patient { User = user4, DateOfBirth = new DateOnly(1995, 5, 5) };

        // --- Schedule (slots) ---
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var slot1 = new Schedule
        {
            Doctor = doctor1,
            Date = tomorrow,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsBusy = true,
            Price = 1500.00m
        }; // used by appointment1
        var slot2 = new Schedule
        {
            Doctor = doctor1,
            Date = tomorrow,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),

            IsBusy = false,
            Price = 1200.00m
        };
        var slot3 = new Schedule
        {
            Doctor = doctor2,
            Date = tomorrow,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsBusy = true,
            Price = 2000.00m
        }; // used by appointment2
        var slot4 = new Schedule
        {
            Doctor = doctor2,
            Date = tomorrow,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            IsBusy = false,
            Price = 1800.00m
        };

        // --- Appointments ---
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

        // --- Payments ---
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

        // Attach payments to appointments
        appointment1.Payments.Add(payment1);
        appointment2.Payments.Add(payment2);

        // Ensure corresponding slots are marked as busy
        slot1.IsBusy = true;
        slot3.IsBusy = true;

        // Add everything to the context in one operation
        context.Users.AddRange(user1, user2, user3, user4);
        context.Doctors.AddRange(doctor1, doctor2);
        context.Patients.AddRange(patient1, patient2);
        context.Schedules.AddRange(slot1, slot2, slot3, slot4);
        context.Appointments.AddRange(appointment1, appointment2);
        context.Payments.AddRange(payment1, payment2);

        context.SaveChanges();
    }
}
