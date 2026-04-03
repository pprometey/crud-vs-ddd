namespace DoctorBooking.DDD.Application.Appointments.Dtos;

public sealed record AppointmentDto(
    Guid Id,
    Guid SlotId,
    Guid PatientId,
    Guid DoctorId,
    DateTime SlotStart,
    decimal SlotPrice,
    string Status,
    decimal PaidTotal,
    decimal RemainingBalance,
    IReadOnlyList<PaymentDto> Payments,
    DateTime CreatedAt);

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    DateTime PaidAt,
    string Status);
