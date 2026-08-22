using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Payment.Application.Features.CreateUpiQr.CreateUpiQr;

namespace Payment.Api.Features.CreateUpiQr;

public class CreateUpiQrEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/order/{orderId:guid}/upi-qr", async (Guid orderId, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var isAdmin = user.HasPermission(Permissions.PaymentsManage);
            var result = await mediator.Send(new Feature.Command(orderId, user.GetUserId(), isAdmin), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization();
    }
}
