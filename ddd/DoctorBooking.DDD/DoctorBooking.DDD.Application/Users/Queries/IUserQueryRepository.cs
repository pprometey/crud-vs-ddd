using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Users.Dtos;

namespace DoctorBooking.DDD.Application.Users.Queries;

/// <summary>
/// Query repository for read-only User operations (no tracking, direct DTO projection).
/// </summary>
public interface IUserQueryRepository
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PagedResult<UserDto>> GetAllPagedAsync(PageRequest request, CancellationToken cancellationToken = default);
}
