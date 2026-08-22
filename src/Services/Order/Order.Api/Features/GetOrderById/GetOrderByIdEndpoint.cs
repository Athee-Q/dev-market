using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Order.Application.Features.GetOrderById.GetOrderById;

namespace Order.Api.Features.GetOrderById;

public class GetOrderByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var isAdmin = user.HasPermission(Permissions.OrdersManage);
            var order = await mediator.Send(new Feature.Query(id, user.GetUserId(), isAdmin), ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("GetOrderById")
        .RequireAuthorization();
    }
}
