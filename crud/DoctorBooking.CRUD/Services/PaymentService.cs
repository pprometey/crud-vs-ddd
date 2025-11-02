using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DoctorBooking.CRUD.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;

    public PaymentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Payment>> GetAllAsync()
    {
        return await _uow.Payments.GetAllAsync(q => q
            .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Appointment).ThenInclude(a => a.Schedule)
        );
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _uow.Payments.GetByIdAsync(id, q => q
            .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Appointment).ThenInclude(a => a.Schedule)
        );
    }

    public async Task<decimal> CreateAsync(Payment p)
    {
        var (appointment, paidPayments) = await PreparePaymentForValidationAsync(p);
        await EnsurePaymentAllowedAsync(p, appointment, paidPayments);

        await _uow.Payments.AddAsync(p);
        await _uow.SaveChangesAsync();

        return ComputeNewPaidSum(paidPayments, p);
    }

    public async Task<decimal> UpdateAsync(Payment p)
    {
        var existing = await LoadPaymentAsync(p.Id);
        var (appointment, paidPayments) = await PreparePaymentForValidationAsync(existing);

        await EnsurePaymentAllowedAsync(p, appointment, paidPayments, existing);

        _uow.Payments.Update(p);
        await _uow.SaveChangesAsync();

        return ComputeNewPaidSum(paidPayments, p, existing);
    }

    public async Task<decimal> DeleteAsync(int id)
    {
        var existing = await LoadPaymentAsync(id);
        var appointment = await LoadAppointmentAsync(existing.AppointmentId);
        var paidPayments = await GetPaidPaymentsForAppointmentAsync(existing.AppointmentId);

        _uow.Payments.Remove(existing);
        await _uow.SaveChangesAsync();

        return paidPayments.Where(x => x.Id != existing.Id).Sum(x => x.Amount);
    }

    public async Task RefundPaidPaymentsForCancellationIfEligibleAsync(int appointmentId)
    {
        var appointment = await LoadAppointmentAsync(appointmentId);
        if (appointment.Status != AppointmentStatus.Cancelled) return;

        var slotStart = appointment.Schedule?.Date.ToDateTime(appointment.Schedule.StartTime)
                        ?? throw new ArgumentException("Appointment.Schedule must be loaded.");

        if ((slotStart - DateTime.Now) < TimeSpan.FromHours(2)) return;

        var paidPayments = await GetPaidPaymentsForAppointmentAsync(appointmentId);
        foreach (var pay in paidPayments)
            pay.Status = PaymentStatus.Refunded;

        if (paidPayments.Count > 0)
        {
            foreach (var pay in paidPayments)
                _uow.Payments.Update(pay);

            await _uow.SaveChangesAsync();
        }
    }

    // --- Private helpers ---

    private async Task<(Appointment appointment, List<Payment> paidPayments)> PreparePaymentForValidationAsync(Payment p)
    {
        var appointment = await LoadAppointmentAsync(p.AppointmentId);

        ValidatePaymentAmount(p);
        ValidatePaymentStatus(p);

        var paidPayments = await GetPaidPaymentsForAppointmentAsync(p.AppointmentId);

        if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot create or update payments for appointments that are Cancelled or Completed.");

        return (appointment, paidPayments);
    }

    private async Task EnsurePaymentAllowedAsync(Payment newPayment, Appointment appointment, List<Payment> paidPayments, Payment? existing = null)
    {
        if (newPayment.Status == PaymentStatus.Paid)
        {
            var otherPatientPaidExists = await _uow.Appointments.AnyAsync(a =>
                a.ScheduleId == appointment.ScheduleId &&
                a.PatientId != appointment.PatientId &&
                a.Payments != null &&
                a.Payments.Any(pay =>
                    pay.Status == PaymentStatus.Paid &&
                    (existing == null || pay.Id != existing.Id))
            );

            if (otherPatientPaidExists)
                throw new InvalidOperationException("Cannot accept payment: another patient already paid for this schedule.");
        }

        var paidExcludingThis = existing != null
            ? paidPayments.Where(x => x.Id != existing.Id).Sum(x => x.Amount)
            : paidPayments.Sum(x => x.Amount);

        var newPaidSum = paidExcludingThis + (newPayment.Status == PaymentStatus.Paid ? newPayment.Amount : 0m);

        if (newPaidSum > appointment.Schedule.Price)
            throw new InvalidOperationException("Total paid payments for this appointment would exceed the schedule price.");
    }

    private decimal ComputeNewPaidSum(IEnumerable<Payment> paidPayments, Payment newPayment, Payment? existing = null)
    {
        var sum = paidPayments
            .Where(x => existing == null || x.Id != existing.Id)
            .Sum(x => x.Amount);

        return sum + (newPayment.Status == PaymentStatus.Paid ? newPayment.Amount : 0m);
    }

    private Task<List<Payment>> GetPaidPaymentsForAppointmentAsync(int appointmentId)
    {
        return _uow.Payments.GetAllAsync(q => q.Where(x => x.AppointmentId == appointmentId && x.Status == PaymentStatus.Paid));
    }

    private async Task<Payment> LoadPaymentAsync(int paymentId)
    {
        return await _uow.Payments.GetByIdAsync(paymentId)
            ?? throw new ArgumentException("Payment not found.");
    }

    private async Task<Appointment> LoadAppointmentAsync(int appointmentId)
    {
        return await _uow.Appointments.GetByIdAsync(appointmentId, q => q
            .Include(a => a.Schedule)
            .Include(a => a.Patient)
            .Include(a => a.Payments)
        ) ?? throw new ArgumentException("Appointment not found.");
    }

    private static void ValidatePaymentAmount(Payment p)
    {
        if (p.Amount <= 0m)
            throw new ArgumentException("Payment.Amount must be greater than 0.");
    }

    private static void ValidatePaymentStatus(Payment p)
    {
        if (p.Status != PaymentStatus.Paid && p.Status != PaymentStatus.Refunded)
            throw new ArgumentException("Payment.Status must be either Paid or Refunded.");
    }
}
