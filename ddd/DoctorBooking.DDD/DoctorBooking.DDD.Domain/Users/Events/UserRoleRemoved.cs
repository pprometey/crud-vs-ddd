using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Users.Events;

public sealed record UserRoleRemoved(UserId UserId, UserRole Role) : DomainEvent;
