using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Payment.Application.Features.SearchPayments.SearchPayments;

namespace Payment.Api.Features.SearchPayments;

/// <summary>Transaction History page. Non-admins are pinned to their own payments regardless of what CustomerId they passed.</summary>
public class SearchPaymentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments", async ([AsParameters] Feature.Query query, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            if (!user.HasPermission(Permissions.PaymentsManage))
                query = query with { CustomerId = user.GetUserId() };

            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization();
    }
}
