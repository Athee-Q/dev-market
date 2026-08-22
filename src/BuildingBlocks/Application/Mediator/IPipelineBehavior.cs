namespace ECommerce.BuildingBlocks.Application.Mediator;

/// <summary>Continuation delegate a pipeline behavior calls to invoke the next behavior, or finally the handler.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Cross-cutting logic that wraps a request's handling — validation, caching, logging, etc.
/// Behaviors run in registration order, each wrapping the next, with the handler itself at the
/// centre. A request type opts into a given behavior structurally (see ICacheableQuery) rather
/// than the behavior needing to know about specific request types.
/// </summary>
public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
