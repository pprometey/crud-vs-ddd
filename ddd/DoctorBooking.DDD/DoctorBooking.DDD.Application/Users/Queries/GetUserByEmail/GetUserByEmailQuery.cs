using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Users.Dtos;

namespace DoctorBooking.DDD.Application.Users.Queries.GetUserByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<Result<UserDto>>;
