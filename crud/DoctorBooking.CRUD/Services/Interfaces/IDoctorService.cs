using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Services.Interfaces;

public interface IDoctorService
{
    Task<List<Doctor>> GetAllAsync();
    Task<Doctor?> GetByIdAsync(int id);
    Task CreateAsync(Doctor d);
    Task UpdateAsync(Doctor d);
    Task DeleteAsync(int id);
}
