namespace DoctorBooking.DDD.Application.Users.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IReadOnlyCollection<string> Roles,
    DateTime CreatedAt);
