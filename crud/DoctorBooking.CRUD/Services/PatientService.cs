using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.CRUD.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _uow;

    public PatientService(IUnitOfWork uow) { _uow = uow; }

    public async Task<List<Patient>> GetAllAsync() => await _uow.Patients.GetAllAsync(q => q.Include(p => p.User));
    public async Task<Patient?> GetByIdAsync(int id) => await _uow.Patients.GetByIdAsync(id, q => q.Include(p => p.User));
    public async Task CreateAsync(Patient p) { await _uow.Patients.AddAsync(p); await _uow.SaveChangesAsync(); }
    public async Task UpdateAsync(Patient p) { _uow.Patients.Update(p); await _uow.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var e = await _uow.Patients.GetByIdAsync(id); if (e != null) { _uow.Patients.Remove(e); await _uow.SaveChangesAsync(); } }
}
