using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.CRUD.Db.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MedicalBookingContext _context;

    public IRepository<User> Users { get; }
    public IRepository<Doctor> Doctors { get; }
    public IRepository<Patient> Patients { get; }
    public IRepository<Schedule> Schedules { get; }
    public IRepository<Appointment> Appointments { get; }
    public IRepository<Payment> Payments { get; }

    public UnitOfWork(MedicalBookingContext context)
    {
        _context = context;
        Users = new Repository<User>(context);
        Doctors = new Repository<Doctor>(context);
        Patients = new Repository<Patient>(context);
        Schedules = new Repository<Schedule>(context);
        Appointments = new Repository<Appointment>(context);
        Payments = new Repository<Payment>(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        // InMemory provider doesn't support transactions. Keep the transactional code visible (commented)
        // so it can be restored for real DB providers, but run the action without transactions to avoid errors.

        // await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            await action();
        //    await tx.CommitAsync();
        }
        catch
        {
        //    await tx.RollbackAsync();
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
