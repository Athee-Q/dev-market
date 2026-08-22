namespace ECommerce.BuildingBlocks.Application.Mediator;

/// <summary>Dispatches a request to its handler, run through any registered pipeline behaviors first.</summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
