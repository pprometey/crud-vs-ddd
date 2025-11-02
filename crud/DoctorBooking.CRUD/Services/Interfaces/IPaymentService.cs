using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Services.Interfaces;

public interface IPaymentService
{
    Task<List<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);

    // Return new total paid sum for the appointment after the operation
    Task<decimal> CreateAsync(Payment p);
    Task<decimal> UpdateAsync(Payment p);
    Task<decimal> DeleteAsync(int id);

    Task RefundPaidPaymentsForCancellationIfEligibleAsync(int appointmentId);
}
