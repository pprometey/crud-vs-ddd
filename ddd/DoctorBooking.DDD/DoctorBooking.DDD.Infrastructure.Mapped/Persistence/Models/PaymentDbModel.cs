namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

/// <summary>
/// Save model для Payment entity - нормализованная модель для хранения в БД
/// </summary>
public class PaymentDbModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public int Status { get; set; }
    
    // FK для EF Core
    public Guid AppointmentId { get; set; }
    public AppointmentDbModel Appointment { get; set; } = null!;
}
