using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Payment.Application.Features.GetPaymentByOrderId.GetPaymentByOrderId;

namespace Payment.Api.Features.GetPaymentByOrderId;

public class GetPaymentByOrderIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/order/{orderId:guid}", async (Guid orderId, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var isAdmin = user.HasPermission(Permissions.PaymentsManage);
            var payment = await mediator.Send(new Feature.Query(orderId, user.GetUserId(), isAdmin), ct);
            return payment is null ? Results.NotFound() : Results.Ok(payment);
        })
        .RequireAuthorization();
    }
}
