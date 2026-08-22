using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Product.Application.Features.SearchProducts.SearchProducts;

namespace Product.Api.Features.SearchProducts;

/// <summary>List/search products. Supports paging via ?page=&pageSize= and filtering via ?search=&categoryId=&isActive=.</summary>
public class SearchProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async ([AsParameters] Feature.Query query, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        });
    }
}
