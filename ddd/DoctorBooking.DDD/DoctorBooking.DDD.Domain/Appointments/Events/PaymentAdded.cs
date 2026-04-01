using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Appointments.Events;

public sealed record PaymentAdded(
    AppointmentId AppointmentId,
    PaymentId PaymentId,
    Money Amount,
    DateTime PaidAt) : DomainEvent;
