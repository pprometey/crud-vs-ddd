using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DoctorBooking.CRUD.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _uow;

    public PatientService(IUnitOfWork uow) { _uow = uow; }

    public async Task<List<Patient>> GetAllAsync() => await _uow.Patients.GetAllAsync(q => q.Include(p => p.User));
    public async Task<Patient?> GetByIdAsync(int id) => await _uow.Patients.GetByIdAsync(id, q => q.Include(p => p.User));

    public async Task CreateAsync(Patient p)
    {
        await EnsureUserExistsAsync(p.UserId, "Patient.UserId must reference an existing User.");
        await EnsureUserNotAlreadyLinkedAsync(p.UserId, null);
        await _uow.Patients.AddAsync(p);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Patient p)
    {
        await EnsureUserExistsAsync(p.UserId, "Patient.UserId must reference an existing User.");
        await EnsureUserNotAlreadyLinkedAsync(p.UserId, p.Id);
        _uow.Patients.Update(p);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id) { var e = await _uow.Patients.GetByIdAsync(id); if (e != null) { _uow.Patients.Remove(e); await _uow.SaveChangesAsync(); } }

    private async Task EnsureUserExistsAsync(int userId, string message)
    {
        var exists = await _uow.Users.AnyAsync(u => u.Id == userId);
        if (!exists)
            throw new ArgumentException(message);
    }

    private async Task EnsureUserNotAlreadyLinkedAsync(int userId, int? excludePatientId)
    {
        var linked = await _uow.Patients.AnyAsync(p =>
            p.UserId == userId &&
            (excludePatientId == null || p.Id != excludePatientId.Value)
        );

        if (linked)
            throw new InvalidOperationException("This User is already associated with another Patient.");
    }
}
