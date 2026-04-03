namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

/// <summary>
/// Save model для Appointment aggregate - нормализованная модель для хранения в БД
/// </summary>
public class AppointmentDbModel
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime SlotStart { get; set; }
    public decimal SlotPriceAmount { get; set; }
    public int Status { get; set; }
    public int Version { get; set; }
    
    // Навигационное свойство для EF Core
    public List<PaymentDbModel> Payments { get; set; } = [];
}
