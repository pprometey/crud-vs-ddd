namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

/// <summary>
/// Save model для Schedule aggregate - нормализованная модель для хранения в БД
/// </summary>
public class ScheduleDbModel
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public int Version { get; set; }
    
    // Навигационное свойство для EF Core
    public List<TimeSlotDbModel> Slots { get; set; } = [];
}
