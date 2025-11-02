using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Services.Interfaces;

public interface IPaymentOrchestrationService
{
    Task CreatePaymentAndMaybeConfirmAsync(Payment p);
    Task UpdatePaymentAndMaybeRecalculateAsync(Payment p);
    Task DeletePaymentAndMaybeRecalculateAsync(int paymentId);
}