using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Payment.Application.Features.HandleWebhook.HandleWebhook;

namespace Payment.Api.Features.HandleWebhook;

/// <summary>
/// Razorpay's server-to-server webhook (payment.captured / payment.failed) — a fallback for when
/// the browser never posts back to Verify (closed tab, network drop, ...). Needs a publicly
/// reachable URL to actually receive calls; see README for local-dev options.
/// </summary>
public class HandleWebhookEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/webhook", async (HttpRequest httpRequest, IMediator mediator, CancellationToken ct) =>
        {
            using var reader = new StreamReader(httpRequest.Body);
            var rawBody = await reader.ReadToEndAsync(ct);
            var signature = httpRequest.Headers["X-Razorpay-Signature"].FirstOrDefault() ?? string.Empty;

            await mediator.Send(new Feature.Command(rawBody, signature), ct);
            return Results.Ok();
        });
    }
}
