using DoctorBooking.DDD.Domain.Users;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Domain.Users.Events;

public sealed record UserRoleAdded(UserId UserId, UserRole Role) : DomainEvent;
