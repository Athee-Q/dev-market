using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Product.Application.Features.GetProductById.GetProductById;

namespace Product.Api.Features.GetProductById;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var product = await mediator.Send(new Feature.Query(id), ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById");
    }
}
