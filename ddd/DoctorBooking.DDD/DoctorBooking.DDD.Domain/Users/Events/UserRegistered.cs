using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Users.Events;

public sealed record UserRegistered(
    UserId UserId,
    Email Email,
    IReadOnlyCollection<UserRole> Roles) : DomainEvent;
