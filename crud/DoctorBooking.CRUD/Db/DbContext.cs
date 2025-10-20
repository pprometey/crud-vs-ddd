using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DoctorBooking.CRUD.Db;

public class MedicalBookingContext : DbContext
{
    public MedicalBookingContext(DbContextOptions<MedicalBookingContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>()
            .HasMany(u => u.Doctors)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Patients)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Doctor
        modelBuilder.Entity<Doctor>()
            .HasMany(d => d.Schedules)
            .WithOne(s => s.Doctor)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Doctor>()
            .HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId);

        // Schedule
        modelBuilder.Entity<Schedule>()
            .HasIndex(s => new { s.DoctorId, s.Date, s.StartTime })
            .IsUnique();

        modelBuilder.Entity<Schedule>()
            .HasMany(s => s.Appointments)
            .WithOne(a => a.Schedule)
            .HasForeignKey(a => a.ScheduleId);

        // Patient
        modelBuilder.Entity<Patient>()
            .HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId);

        // Appointment
        modelBuilder.Entity<Appointment>()
            .HasMany(a => a.Payments)
            .WithOne(p => p.Appointment)
            .HasForeignKey(p => p.AppointmentId);

        // Enum mapping (store as int)
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<int>();

        modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<int>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<int>();
    }
}


//// Design-time DbContext factory for migrations and scaffolding
//public class MedicalBookingContextFactory : IDesignTimeDbContextFactory<MedicalBookingContext>
//{
//    public MedicalBookingContext CreateDbContext(string[] args)
//    {
//        var optionsBuilder = new DbContextOptionsBuilder<MedicalBookingContext>();
//        optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=MedicalBookingDb;Trusted_Connection=True;");

//        return new MedicalBookingContext(optionsBuilder.Options);
//    }
//}