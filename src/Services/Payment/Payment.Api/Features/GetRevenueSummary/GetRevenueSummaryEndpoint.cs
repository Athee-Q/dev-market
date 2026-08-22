using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Payment.Application.Features.GetRevenueSummary.GetRevenueSummary;

namespace Payment.Api.Features.GetRevenueSummary;

public class GetRevenueSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/summary", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new Feature.Query(), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(Permissions.PaymentsManage);
    }
}
