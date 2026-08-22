using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Order.Application.Features.CreateOrder.CreateOrder;

namespace Order.Api.Features.CreateOrder;

/// <summary>Request body carries only the items — CustomerId comes from the caller's JWT, never the client, so nobody can place an order "as" someone else.</summary>
public record CreateOrderBody(IReadOnlyCollection<Feature.ItemInput> Items);

public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderBody body, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var created = await mediator.Send(new Feature.Command(user.GetUserId(), body.Items), ct);
            return Results.CreatedAtRoute("GetOrderById", new { id = created.Id }, created);
        })
        .RequireAuthorization();
    }
}
