using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Product.Application.Features.UpdateProduct.UpdateProduct;

namespace Product.Api.Features.UpdateProduct;

/// <summary>Request body shape — same fields as Feature.Command minus Id, which comes from the route.</summary>
public record UpdateProductBody(
    Guid CategoryId, string Name, string Description, decimal Price, string SKU, bool IsActive,
    Product.Domain.ProductType ProductType, Product.Domain.PricingModel PricingModel, string? AssetUrl);

public class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductBody body, IMediator mediator, CancellationToken ct) =>
        {
            var command = new Feature.Command(
                id, body.CategoryId, body.Name, body.Description, body.Price, body.SKU, body.IsActive,
                body.ProductType, body.PricingModel, body.AssetUrl);
            var updated = await mediator.Send(command, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization(Permissions.ProductsManage);
    }
}
