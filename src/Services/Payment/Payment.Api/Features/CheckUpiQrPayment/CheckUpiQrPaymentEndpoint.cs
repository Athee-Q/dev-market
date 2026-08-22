using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Payment.Application.Features.CheckUpiQrPayment.CheckUpiQrPayment;

namespace Payment.Api.Features.CheckUpiQrPayment;

public class CheckUpiQrPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/order/{orderId:guid}/upi-qr/status", async (Guid orderId, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var isAdmin = user.HasPermission(Permissions.PaymentsManage);
            var result = await mediator.Send(new Feature.Query(orderId, user.GetUserId(), isAdmin), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization();
    }
}
