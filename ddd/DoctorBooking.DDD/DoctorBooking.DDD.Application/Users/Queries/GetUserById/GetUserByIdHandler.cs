using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Users.Dtos;
using DoctorBooking.DDD.Application.Users.Errors;

namespace DoctorBooking.DDD.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserQueryRepository _queryRepo;

    public GetUserByIdHandler(IUserQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<UserDto>> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _queryRepo.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.Failure(new ValidationError(
                nameof(query.UserId),
                AppErrorCodes.User.NotFound,
                UserMessages.Msg(AppErrorCodes.User.NotFound, query.UserId)));
        }

        return Result<UserDto>.Success(user);
    }
}
