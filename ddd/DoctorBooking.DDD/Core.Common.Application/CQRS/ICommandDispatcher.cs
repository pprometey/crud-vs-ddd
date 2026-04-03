using Core.Common.Application;
using Mediator;

namespace Core.Common.Application.CQRS;

/// <summary>
/// Dispatcher for executing commands.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches a command to its handler.
    /// </summary>
    ValueTask<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of command dispatcher using Mediator library.
/// </summary>
public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IMediator _mediator;

    public CommandDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public ValueTask<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        return _mediator.Send(command, cancellationToken);
    }
}
