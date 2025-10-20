using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Services.Interfaces;

public interface IPaymentService
{
    Task<List<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task CreateAsync(Payment p);
    Task UpdateAsync(Payment p);
    Task DeleteAsync(int id);
}
