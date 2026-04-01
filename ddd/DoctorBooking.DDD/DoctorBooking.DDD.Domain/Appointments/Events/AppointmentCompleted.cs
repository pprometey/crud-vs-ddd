using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Appointments.Events;

public sealed record AppointmentCompleted(AppointmentId AppointmentId, UserId DoctorId) : DomainEvent;
