using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
        await EnsureUserExistsAsync(d.UserId, "Doctor.UserId must reference an existing User.");
        await EnsureUserNotAlreadyLinkedAsync(d.UserId, null);
        await _uow.Doctors.AddAsync(d);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Doctor d)
    {
        await EnsureUserExistsAsync(d.UserId, "Doctor.UserId must reference an existing User.");
        await EnsureUserNotAlreadyLinkedAsync(d.UserId, d.Id);
        _uow.Doctors.Update(d);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _uow.Doctors.GetByIdAsync(id);
        if (e != null) { _uow.Doctors.Remove(e); await _uow.SaveChangesAsync(); }
    }

    private async Task EnsureUserExistsAsync(int userId, string message)
    {
        var exists = await _uow.Users.AnyAsync(u => u.Id == userId);
        if (!exists)
            throw new ArgumentException(message);
    }

    private async Task EnsureUserNotAlreadyLinkedAsync(int userId, int? excludeDoctorId)
    {
        var linked = await _uow.Doctors.AnyAsync(d =>
            d.UserId == userId &&
            (excludeDoctorId == null || d.Id != excludeDoctorId.Value)
        );

        if (linked)
            throw new InvalidOperationException("This User is already associated with another Doctor.");
    }
}
