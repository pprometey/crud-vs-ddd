using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Services.Interfaces;

public interface IPatientService
{
    Task<List<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task CreateAsync(Patient p);
    Task UpdateAsync(Patient p);
    Task DeleteAsync(int id);
}
