namespace DoctorBooking.DDD.Domain.Appointments;

public enum AppointmentStatus
{
    Planned,
    Confirmed,
    Cancelled,
    Completed
}

public static class AppointmentStatusExtensions
{
    public static bool AllowsPayment(this AppointmentStatus status)
        => status == AppointmentStatus.Planned;

    public static bool IsCancellable(this AppointmentStatus status)
        => status is AppointmentStatus.Planned or AppointmentStatus.Confirmed;

    public static bool IsFinal(this AppointmentStatus status)
        => status is AppointmentStatus.Cancelled or AppointmentStatus.Completed;
}
