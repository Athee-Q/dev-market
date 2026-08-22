using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Identity.Application.Features.Login.Login;

namespace Identity.Api.Features.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/login", async (Feature.Command command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        });
    }
}
