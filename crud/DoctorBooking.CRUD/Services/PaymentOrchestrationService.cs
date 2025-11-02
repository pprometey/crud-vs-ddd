using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DoctorBooking.CRUD.Services;

public class PaymentOrchestrationService : IPaymentOrchestrationService
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _paymentService;

    public PaymentOrchestrationService(IUnitOfWork uow, IPaymentService paymentService)
    {
        _uow = uow;
        _paymentService = paymentService;
    }

    public async Task CreatePaymentAndMaybeConfirmAsync(Payment p)
    {
        await ExecuteWithAppointmentAsync(p.AppointmentId, async appointment =>
        {
            var newPaidSum = await _paymentService.CreateAsync(p);
            await UpdateAppointmentStatusAndScheduleAsync(appointment, newPaidSum);
        });
    }

    public async Task UpdatePaymentAndMaybeRecalculateAsync(Payment p)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var existingPayment = await _uow.Payments.GetByIdAsync(p.Id);
            if (existingPayment == null) throw new ArgumentException("Payment not found.");

            var appointment = await _uow.Appointments.GetByIdAsync(existingPayment.AppointmentId, q => q.Include(a => a.Schedule));
            if (appointment == null) throw new ArgumentException("Associated appointment not found.");

            // Ensure no other patient already has Paid for this schedule
            if (p.Status == PaymentStatus.Paid)
            {
                var otherPatientPaidExists = await _uow.Appointments.AnyAsync(a =>
                    a.ScheduleId == appointment.ScheduleId &&
                    a.PatientId != appointment.PatientId &&
                    a.Payments != null &&
                    a.Payments.Any(pay => pay.Status == PaymentStatus.Paid && pay.Id != existingPayment.Id)
                );
                if (otherPatientPaidExists)
                    throw new InvalidOperationException("Cannot accept payment: another patient already paid for this schedule.");
            }

            var newPaidSum = await _paymentService.UpdateAsync(p);
            await UpdateAppointmentStatusAndScheduleAsync(appointment, newPaidSum);
        });
    }

    public async Task DeletePaymentAndMaybeRecalculateAsync(int paymentId)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var payment = await _uow.Payments.GetByIdAsync(paymentId);
            if (payment == null) return;

            var appointment = await _uow.Appointments.GetByIdAsync(payment.AppointmentId, q => q.Include(a => a.Schedule));
            if (appointment == null) throw new ArgumentException("Appointment not found.");

            var newPaidSum = await _paymentService.DeleteAsync(paymentId);
            await UpdateAppointmentStatusAndScheduleAsync(appointment, newPaidSum);
        });
    }

    // --- Private helpers ---

    private async Task ExecuteWithAppointmentAsync(int appointmentId, Func<Appointment, Task> action)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var appointment = await _uow.Appointments.GetByIdAsync(appointmentId, q => q.Include(a => a.Schedule));
            if (appointment == null) throw new ArgumentException("Appointment not found.");
            await action(appointment);
        });
    }

    private async Task UpdateAppointmentStatusAndScheduleAsync(Appointment appointment, decimal paidSum)
    {
        var previousStatus = appointment.Status;

        if (paidSum >= appointment.Schedule.Price && appointment.Status == AppointmentStatus.Scheduled)
            appointment.Status = AppointmentStatus.Confirmed;
        else if (paidSum < appointment.Schedule.Price && appointment.Status == AppointmentStatus.Confirmed)
            appointment.Status = AppointmentStatus.Scheduled;

        if (appointment.Status != previousStatus)
        {
            _uow.Appointments.Update(appointment);
            await _uow.SaveChangesAsync();
        }

        await UpdateScheduleIsBusyAsync(appointment.ScheduleId);
    }

    private async Task UpdateScheduleIsBusyAsync(int scheduleId)
    {
        var hasActive = await _uow.Appointments.AnyAsync(a =>
            a.ScheduleId == scheduleId &&
            (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
        );

        var schedule = await _uow.Schedules.GetByIdAsync(scheduleId);
        if (schedule == null) throw new ArgumentException("Schedule not found.");

        if (schedule.IsBusy != hasActive)
        {
            schedule.IsBusy = hasActive;
            _uow.Schedules.Update(schedule);
            await _uow.SaveChangesAsync();
        }
    }
}
