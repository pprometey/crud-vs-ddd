using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.CRUD.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;

    public PaymentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Payment>> GetAllAsync()
    {
        return await _uow.Payments.GetAllAsync(q => q
            .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Appointment).ThenInclude(a => a.Schedule)
        );
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _uow.Payments.GetByIdAsync(id, q => q
            .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Appointment).ThenInclude(a => a.Schedule)
        );
    }

    public async Task CreateAsync(Payment p)
    {
        await _uow.Payments.AddAsync(p);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment p)
    {
        _uow.Payments.Update(p);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _uow.Payments.GetByIdAsync(id);
        if (e != null) { _uow.Payments.Remove(e); await _uow.SaveChangesAsync(); }
    }
}
