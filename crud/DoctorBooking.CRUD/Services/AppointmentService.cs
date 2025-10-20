using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.CRUD.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;

    public AppointmentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _uow.Appointments.GetAllAsync(q => q
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Schedule)
        );
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _uow.Appointments.GetByIdAsync(id, q => q
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Schedule)
        );
    }

    public async Task CreateAsync(Appointment a)
    {
        await _uow.Appointments.AddAsync(a);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Appointment a)
    {
        _uow.Appointments.Update(a);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _uow.Appointments.GetByIdAsync(id);
        if (e != null) { _uow.Appointments.Remove(e); await _uow.SaveChangesAsync(); }
    }
}
