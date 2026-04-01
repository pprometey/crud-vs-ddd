using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Domain.Schedules;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Appointments.Events;

public sealed record AppointmentCreated(
    AppointmentId AppointmentId,
    UserId PatientId,
    UserId DoctorId,
    TimeSlotId SlotId) : DomainEvent;
