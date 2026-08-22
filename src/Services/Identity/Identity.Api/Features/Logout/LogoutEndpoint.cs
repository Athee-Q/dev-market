using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Identity.Application.Features.Logout.Logout;

namespace Identity.Api.Features.Logout;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/logout", async (Feature.Command command, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(command, ct);
            return Results.NoContent();
        });
    }
}
