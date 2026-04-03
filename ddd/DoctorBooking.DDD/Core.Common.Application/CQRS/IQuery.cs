using Mediator;

namespace Core.Common.Application.CQRS;

/// <summary>
/// Marker interface for queries (read operations without side effects).
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
