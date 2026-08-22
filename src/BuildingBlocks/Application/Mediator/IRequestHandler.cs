namespace ECommerce.BuildingBlocks.Application.Mediator;

/// <summary>The single handler for a given <see cref="IRequest{TResponse}"/> — one per vertical slice.</summary>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
