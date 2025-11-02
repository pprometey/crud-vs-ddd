using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoctorBooking.CRUD.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _paymentService;

    public AppointmentService(IUnitOfWork uow, IPaymentService paymentService)
    {
        _uow = uow;
        _paymentService = paymentService;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _uow.Appointments.GetAllAsync(q => q
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Schedule)
        );
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _uow.Appointments.GetByIdAsync(id, q => q
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Schedule)
        );
    }

    public async Task CreateAsync(Appointment a)
    {
        a.Status = AppointmentStatus.Scheduled;

        await ExecuteWithAppointmentAndScheduleAsync(a, async (appt, schedule) =>
        {
            await EnsurePatientHasNoOtherActiveAppointmentsAsync(appt);

            if (schedule.IsBusy)
                throw new InvalidOperationException("Cannot create appointment: schedule is busy.");

            RunValidations(appt, schedule, await LoadPatientWithUser(appt.PatientId));

            await _uow.Appointments.AddAsync(appt);
            await _uow.SaveChangesAsync();
        });
    }

    public async Task UpdateAsync(Appointment a)
    {
        var existing = await LoadAppointmentWithRelations(a.Id);

        EnsureStatusChangeIsValid(existing, a);

        await ExecuteWithAppointmentAndScheduleAsync(a, async (appt, schedule) =>
        {
            await EnsurePatientHasNoOtherActiveAppointmentsAsync(appt, existing);

            RunValidations(appt, schedule, existing.Patient ?? await LoadPatientWithUser(appt.PatientId));

            if (appt.Status != existing.Status)
            {
                await _uow.ExecuteInTransactionAsync(async () =>
                {
                    _uow.Appointments.Update(appt);
                    await _uow.SaveChangesAsync();

                    if (appt.Status == AppointmentStatus.Cancelled)
                        await _paymentService.RefundPaidPaymentsForCancellationIfEligibleAsync(appt.Id);

                    await UpdateScheduleIsBusyAsync(appt.ScheduleId);
                });
                return;
            }

            _uow.Appointments.Update(appt);
            await _uow.SaveChangesAsync();
        });
    }

    public async Task DeleteAsync(int id)
    {
        var appt = await _uow.Appointments.GetByIdAsync(id);
        if (appt == null) return;

        _uow.Appointments.Remove(appt);
        await _uow.SaveChangesAsync();

        await UpdateScheduleIsBusyAsync(appt.ScheduleId);
    }

    // --- Private helpers ---
    private async Task ExecuteWithAppointmentAndScheduleAsync(Appointment a, Func<Appointment, Schedule, Task> action)
    {
        var schedule = await LoadScheduleWithDoctor(a.ScheduleId);
        await action(a, schedule);
    }

    private static void RunValidations(Appointment a, Schedule s, Patient p)
    {
        ValidateDoctorMatchesSchedule(a, s);
        ValidateScheduledTimeWithinBounds(a, s);
        ValidateDoctorAndPatientNotSameUser(s, p);
    }

    private async Task EnsurePatientHasNoOtherActiveAppointmentsAsync(Appointment a, Appointment? existing = null)
    {
        var existingId = existing?.Id ?? 0;

        var hasOtherActive = await _uow.Appointments.AnyAsync(x =>
            x.Id != existingId &&
            x.PatientId == a.PatientId &&
            x.ScheduleId == a.ScheduleId &&
            (x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.Confirmed)
        );

        if (hasOtherActive)
            throw new InvalidOperationException("Patient already has an active appointment (Scheduled or Confirmed) for this schedule.");
    }

    private static void ValidateDoctorMatchesSchedule(Appointment a, Schedule s)
    {
        if (a.DoctorId != s.DoctorId)
            throw new ArgumentException("Appointment.DoctorId must match Schedule.DoctorId.");
    }

    private static void ValidateScheduledTimeWithinBounds(Appointment a, Schedule s)
    {
        if (a.ScheduledTime < DateTime.Now)
            throw new ArgumentException("Appointment.ScheduledTime cannot be in the past.");

        var scheduleStart = s.Date.ToDateTime(s.StartTime);
        var scheduleEnd = s.Date.ToDateTime(s.EndTime);

        if (a.ScheduledTime < scheduleStart || a.ScheduledTime > scheduleEnd)
            throw new ArgumentException("Appointment.ScheduledTime must be within the schedule time interval.");
    }

    private static void ValidateDoctorAndPatientNotSameUser(Schedule s, Patient p)
    {
        var doctorUserId = s.Doctor?.User?.Id;
        var patientUserId = p.User?.Id;
        if (doctorUserId != null && patientUserId != null && doctorUserId == patientUserId)
            throw new ArgumentException("Doctor.UserId cannot be equal to Patient.UserId.");
    }

    private static void EnsureStatusChangeIsValid(Appointment existing, Appointment updated)
    {
        if ((existing.Status == AppointmentStatus.Completed || existing.Status == AppointmentStatus.Cancelled) &&
            updated.Status != existing.Status)
            throw new InvalidOperationException("Cannot change status after appointment reached Completed or Cancelled.");

        if (updated.Status == AppointmentStatus.Completed && existing.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Appointment can be set to Completed only from Confirmed status.");
    }

    private async Task<Appointment> LoadAppointmentWithRelations(int id)
    {
        return await _uow.Appointments.GetByIdAsync(id, q => q
            .Include(x => x.Schedule).ThenInclude(s => s.Doctor).ThenInclude(d => d.User)
            .Include(x => x.Patient).ThenInclude(p => p.User)
        ) ?? throw new ArgumentException("Appointment not found.");
    }

    private async Task<Schedule> LoadScheduleWithDoctor(int id)
    {
        return await _uow.Schedules.GetByIdAsync(id, q => q
            .Include(s => s.Doctor).ThenInclude(d => d.User)
        ) ?? throw new ArgumentException("Schedule not found.");
    }

    private async Task<Patient> LoadPatientWithUser(int id)
    {
        return await _uow.Patients.GetByIdAsync(id, q => q.Include(p => p.User))
            ?? throw new ArgumentException("Patient not found.");
    }

    private async Task UpdateScheduleIsBusyAsync(int scheduleId)
    {
        var hasActive = await _uow.Appointments.AnyAsync(x =>
            x.ScheduleId == scheduleId && x.Status == AppointmentStatus.Confirmed
        );

        var sched = await _uow.Schedules.GetByIdAsync(scheduleId);
        if (sched != null)
        {
            sched.IsBusy = hasActive;
            _uow.Schedules.Update(sched);
            await _uow.SaveChangesAsync();
        }
    }
}
