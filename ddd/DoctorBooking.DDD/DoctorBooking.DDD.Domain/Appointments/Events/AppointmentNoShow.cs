using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Appointments.Events;

public sealed record AppointmentNoShow(AppointmentId AppointmentId, UserId PatientId) : DomainEvent;
