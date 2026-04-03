using Core.Common.Application;
using Mediator;

namespace Core.Common.Application.CQRS;

/// <summary>
/// Dispatcher for executing queries.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches a query to its handler.
    /// </summary>
    ValueTask<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of query dispatcher using Mediator library.
/// </summary>
public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IMediator _mediator;

    public QueryDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public ValueTask<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return _mediator.Send(query, cancellationToken);
    }
}
