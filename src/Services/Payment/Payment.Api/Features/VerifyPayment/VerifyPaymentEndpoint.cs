using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Payment.Application.Features.VerifyPayment.VerifyPayment;

namespace Payment.Api.Features.VerifyPayment;

public record VerifyPaymentBody(Guid OrderId, string RazorpayOrderId, string RazorpayPaymentId, string RazorpaySignature);

/// <summary>Verifies the signature Razorpay Checkout returns to the browser on success and settles the payment.</summary>
public class VerifyPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/verify", async (VerifyPaymentBody body, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var isAdmin = user.HasPermission(Permissions.PaymentsManage);
            var command = new Feature.Command(
                body.OrderId, body.RazorpayOrderId, body.RazorpayPaymentId, body.RazorpaySignature, user.GetUserId(), isAdmin);
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization();
    }
}
