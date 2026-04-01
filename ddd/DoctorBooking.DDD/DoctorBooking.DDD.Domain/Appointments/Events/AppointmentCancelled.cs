using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Appointments.Events;

public sealed record AppointmentCancelled(
    AppointmentId AppointmentId,
    UserId CancelledBy,
    bool ShouldRefund,
    IReadOnlyList<Payment> Payments) : DomainEvent;
