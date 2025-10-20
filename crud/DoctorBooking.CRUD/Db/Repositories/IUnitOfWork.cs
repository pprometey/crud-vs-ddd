namespace DoctorBooking.CRUD.Db.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Doctor> Doctors { get; }
    IRepository<Patient> Patients { get; }
    IRepository<Schedule> Schedules { get; }
    IRepository<Appointment> Appointments { get; }
    IRepository<Payment> Payments { get; }

    Task<int> SaveChangesAsync();
}
