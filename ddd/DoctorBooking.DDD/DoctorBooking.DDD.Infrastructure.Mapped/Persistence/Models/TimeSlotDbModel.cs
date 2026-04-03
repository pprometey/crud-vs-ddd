namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

/// <summary>
/// Save model для TimeSlot entity - нормализованная модель для хранения в БД
/// </summary>
public class TimeSlotDbModel
{
    public Guid Id { get; set; }
    public DateTime Start { get; set; }
    public long DurationTicks { get; set; }
    public decimal PriceAmount { get; set; }
    public Guid DoctorId { get; set; }
    
    // FK для EF Core
    public Guid ScheduleId { get; set; }
    public ScheduleDbModel Schedule { get; set; } = null!;
}
