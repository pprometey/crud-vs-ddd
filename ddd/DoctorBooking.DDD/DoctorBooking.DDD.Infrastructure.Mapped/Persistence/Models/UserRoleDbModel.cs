namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

/// <summary>
/// Save model для User roles - нормализованная модель для хранения в БД
/// </summary>
public class UserRoleDbModel
{
    public Guid Id { get; set; }
    public int Role { get; set; }
    
    // FK для EF Core
    public Guid UserId { get; set; }
    public UserDbModel User { get; set; } = null!;
}
