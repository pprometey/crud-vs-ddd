namespace DoctorBooking.CRUD.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public UserRole Role { get; set; }

    public ICollection<Doctor>? Doctors { get; set; }
    public ICollection<Patient>? Patients { get; set; }
}

public class Doctor
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Specialization { get; set; } = null!;

    public User User { get; set; } = null!;
    public ICollection<Schedule>? Schedules { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
}

public class Patient
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public User User { get; set; } = null!; 
    public ICollection<Appointment>? Appointments { get; set; }
}

public class Schedule
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public ICollection<Appointment>? Appointments { get; set; }
}

public class Appointment
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public int ScheduleId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
    public ICollection<Payment>? Payments { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaymentDate { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
