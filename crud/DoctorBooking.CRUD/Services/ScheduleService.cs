using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.CRUD.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _uow;

    public ScheduleService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Schedule>> GetAllAsync()
    {
        return await _uow.Schedules.GetAllAsync(q => q.Include(s => s.Doctor).ThenInclude(d => d.User));
    }

    public async Task<Schedule?> GetByIdAsync(int id)
    {
        return await _uow.Schedules.GetByIdAsync(id, q => q.Include(s => s.Doctor).ThenInclude(d => d.User));
    }

    public async Task CreateAsync(Schedule s)
    {
        await _uow.Schedules.AddAsync(s);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Schedule s)
    {
        _uow.Schedules.Update(s);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _uow.Schedules.GetByIdAsync(id);
        if (e != null) { _uow.Schedules.Remove(e); await _uow.SaveChangesAsync(); }
    }
}
