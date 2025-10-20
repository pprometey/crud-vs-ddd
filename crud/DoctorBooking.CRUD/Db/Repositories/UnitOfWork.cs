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

    public void Dispose()
    {
        _context.Dispose();
    }
}
