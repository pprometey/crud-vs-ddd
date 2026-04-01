using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Appointments.Events;

public sealed record AppointmentConfirmed(
    AppointmentId AppointmentId,
    UserId PatientId,
    UserId DoctorId) : DomainEvent;
