using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Users.Dtos;

namespace DoctorBooking.DDD.Application.Users.Queries.GetUsersPaged;

public sealed class GetUsersPagedHandler
    : IQueryHandler<GetUsersPagedQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUserQueryRepository _queryRepo;

    public GetUsersPagedHandler(IUserQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<PagedResult<UserDto>>> Handle(
        GetUsersPagedQuery query,
        CancellationToken cancellationToken)
    {
        var pagedUsers = await _queryRepo.GetAllPagedAsync(query.PageRequest, cancellationToken);
        return Result<PagedResult<UserDto>>.Success(pagedUsers);
    }
}
