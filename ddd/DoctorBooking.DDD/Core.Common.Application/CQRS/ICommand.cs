using Mediator;

namespace Core.Common.Application.CQRS;

/// <summary>
/// Marker interface for commands (write operations with side effects).
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;
