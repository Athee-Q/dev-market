using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Order.Application.Features.SearchOrders.SearchOrders;

namespace Order.Api.Features.SearchOrders;

public class SearchOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders", async ([AsParameters] Feature.Query query, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            // Only OrdersManage (Admin) can see/filter across customers — everyone else is
            // pinned to their own orders regardless of what CustomerId they passed.
            if (!user.HasPermission(Permissions.OrdersManage))
                query = query with { CustomerId = user.GetUserId() };

            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization();
    }
}
