using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Identity.Application.Features.Register.Register;

namespace Identity.Api.Features.Register;

public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/register", async (Feature.Command command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        });
    }
}
