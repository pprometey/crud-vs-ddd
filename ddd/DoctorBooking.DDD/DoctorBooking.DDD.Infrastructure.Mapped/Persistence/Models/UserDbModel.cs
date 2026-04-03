namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

/// <summary>
/// Save model для User aggregate - нормализованная модель для хранения в БД
/// </summary>
public class UserDbModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Version { get; set; }
    
    // Навигационное свойство для EF Core
    public List<UserRoleDbModel> Roles { get; set; } = [];
}
