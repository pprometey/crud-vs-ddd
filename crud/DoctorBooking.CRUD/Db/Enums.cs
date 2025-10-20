namespace DoctorBooking.CRUD.Db;

public enum UserRole
{
    Patient,
    Doctor,
    Admin
}

public enum AppointmentStatus
{
    Scheduled,
    Cancelled,
    Completed
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Refunded
}
