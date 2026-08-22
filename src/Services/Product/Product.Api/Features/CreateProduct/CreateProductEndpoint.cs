using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Product.Application.Features.CreateProduct.CreateProduct;

namespace Product.Api.Features.CreateProduct;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async (Feature.Command command, IMediator mediator, CancellationToken ct) =>
        {
            var created = await mediator.Send(command, ct);
            return Results.CreatedAtRoute("GetProductById", new { id = created.Id }, created);
        })
        .RequireAuthorization(Permissions.ProductsManage);
    }
}
