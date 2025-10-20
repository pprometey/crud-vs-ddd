using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Services.Interfaces;

public interface IAppointmentService
{
    Task<List<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task CreateAsync(Appointment a);
    Task UpdateAsync(Appointment a);
    Task DeleteAsync(int id);
}
