using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.CRUD.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _uow;

    public DoctorService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Doctor>> GetAllAsync()
    {
        return await _uow.Doctors.GetAllAsync(q => q.Include(d => d.User));
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        return await _uow.Doctors.GetByIdAsync(id, q => q.Include(d => d.User));
    }

    public async Task CreateAsync(Doctor d)
    {
        await _uow.Doctors.AddAsync(d);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Doctor d)
    {
        _uow.Doctors.Update(d);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _uow.Doctors.GetByIdAsync(id);
        if (e != null) { _uow.Doctors.Remove(e); await _uow.SaveChangesAsync(); }
    }
}
