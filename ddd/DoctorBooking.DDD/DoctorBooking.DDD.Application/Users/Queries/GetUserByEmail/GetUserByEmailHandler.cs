using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Users.Dtos;
using DoctorBooking.DDD.Application.Users.Errors;

namespace DoctorBooking.DDD.Application.Users.Queries.GetUserByEmail;

public sealed class GetUserByEmailHandler : IQueryHandler<GetUserByEmailQuery, Result<UserDto>>
{
    private readonly IUserQueryRepository _queryRepo;

    public GetUserByEmailHandler(IUserQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async ValueTask<Result<UserDto>> Handle(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _queryRepo.GetByEmailAsync(query.Email, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.Failure(new ValidationError(
                nameof(query.Email),
                AppErrorCodes.User.NotFound,
                UserMessages.Msg(AppErrorCodes.User.NotFound, query.Email)));
        }

        return Result<UserDto>.Success(user);
    }
}
