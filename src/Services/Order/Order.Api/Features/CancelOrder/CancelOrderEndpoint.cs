using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Order.Application.Features.CancelOrder.CancelOrder;

namespace Order.Api.Features.CancelOrder;

public class CancelOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/cancel", async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var isAdmin = user.HasPermission(Permissions.OrdersManage);
            var cancelled = await mediator.Send(new Feature.Command(id, user.GetUserId(), isAdmin), ct);
            return cancelled is null ? Results.NotFound() : Results.Ok(cancelled);
        })
        .RequireAuthorization();
    }
}
