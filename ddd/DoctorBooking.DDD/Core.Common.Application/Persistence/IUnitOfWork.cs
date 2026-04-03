namespace Core.Common.Application.Persistence;

/// <summary>
/// Unit of Work pattern for managing transaction boundaries in application handlers.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
