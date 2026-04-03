using Mediator;

namespace Core.Common.Application.CQRS;

/// <summary>
/// Handler for queries (read operations without side effects).
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
