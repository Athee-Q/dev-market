using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Identity.Application.Features.GetMe.GetMe;

namespace Identity.Api.Features.GetMe;

public class GetMeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/me", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var me = await mediator.Send(new Feature.Query(user.GetUserId()), ct);
            return me is null ? Results.NotFound() : Results.Ok(me);
        })
        .RequireAuthorization();
    }
}
