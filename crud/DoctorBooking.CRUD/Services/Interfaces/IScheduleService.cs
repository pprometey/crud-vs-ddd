using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Services.Interfaces;

public interface IScheduleService
{
    Task<List<Schedule>> GetAllAsync();
    Task<Schedule?> GetByIdAsync(int id);
    Task CreateAsync(Schedule s);
    Task UpdateAsync(Schedule s);
    Task DeleteAsync(int id);
}
