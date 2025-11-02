using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoctorBooking.CRUD.Db;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;

    public ICollection<Doctor>? Doctors { get; set; }
    public ICollection<Patient>? Patients { get; set; }
}

public class Doctor
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Specialization { get; set; } = null!;

    [ValidateNever]
    public User User { get; set; } = null!;
    public ICollection<Schedule>? Schedules { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
}

public class Patient
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly DateOfBirth { get; set; }

    [ValidateNever]
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

    public bool IsBusy { get; set; }

    public decimal Price { get; set; }

    [ValidateNever]
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

    [ValidateNever]
    public Doctor Doctor { get; set; } = null!;
    [ValidateNever]
    public Patient Patient { get; set; } = null!;
    [ValidateNever]
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

    [ValidateNever]
    public Appointment Appointment { get; set; } = null!;
}


public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

