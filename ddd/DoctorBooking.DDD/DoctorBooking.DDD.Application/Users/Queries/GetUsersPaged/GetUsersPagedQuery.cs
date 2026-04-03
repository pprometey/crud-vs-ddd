using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Users.Dtos;

namespace DoctorBooking.DDD.Application.Users.Queries.GetUsersPaged;

public sealed record GetUsersPagedQuery(PageRequest PageRequest) 
    : IQuery<Result<PagedResult<UserDto>>>;
