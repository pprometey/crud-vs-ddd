using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        await ValidateAndPrepareScheduleAsync(s);

        await _uow.Schedules.AddAsync(s);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Schedule s)
    {
        await EnsureNotLockedAsync(s.Id);
        await ValidateAndPrepareScheduleAsync(s, s.Id);

        _uow.Schedules.Update(s);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await EnsureNotLockedAsync(id);

        var schedule = await _uow.Schedules.GetByIdAsync(id);
        if (schedule != null)
        {
            _uow.Schedules.Remove(schedule);
            await _uow.SaveChangesAsync();
        }
    }

    // --- Private helpers ---

    private async Task ValidateAndPrepareScheduleAsync(Schedule s, int? excludeScheduleId = null)
    {
        ValidateScheduleTimes(s);
        ValidateScheduleDateNotPast(s);
        ValidateSchedulePrice(s);

        var overlaps = await _uow.Schedules.AnyAsync(x =>
            x.DoctorId == s.DoctorId &&
            x.Date == s.Date &&
            (!excludeScheduleId.HasValue || x.Id != excludeScheduleId.Value) &&
            !(s.EndTime <= x.StartTime || s.StartTime >= x.EndTime)
        );

        if (overlaps)
            throw new ArgumentException("Schedule time overlaps with an existing schedule for the same doctor on the same date.");

        s.IsBusy = await _uow.Appointments.AnyAsync(a =>
            a.ScheduleId == s.Id &&
            (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
        );
    }

    private async Task EnsureNotLockedAsync(int scheduleId)
    {
        var hasLockedAppointments = await _uow.Appointments.AnyAsync(a =>
            a.ScheduleId == scheduleId &&
            (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed)
        );

        if (hasLockedAppointments)
            throw new InvalidOperationException("Cannot modify or delete schedule because there is at least one related appointment with status Confirmed or Completed.");
    }

    private static void ValidateScheduleTimes(Schedule s)
    {
        if (s.StartTime >= s.EndTime)
            throw new ArgumentException("Schedule.StartTime must be earlier than Schedule.EndTime.");
    }

    private static void ValidateScheduleDateNotPast(Schedule s)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (s.Date < today)
            throw new ArgumentException("Schedule.Date cannot be in the past.");
    }

    private static void ValidateSchedulePrice(Schedule s)
    {
        if (s.Price < 0m)
            throw new ArgumentException("Schedule.Price must be greater than or equal to 0.");
    }
}
