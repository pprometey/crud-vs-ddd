using Mediator;

namespace Core.Common.Application.CQRS;

/// <summary>
/// Handler for commands (write operations with side effects).
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;
