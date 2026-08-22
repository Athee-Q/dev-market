using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Identity.Application.Features.AssignRole.AssignRole;

namespace Identity.Api.Features.AssignRole;

public class AssignRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/users/{id:guid}/roles", async (Guid id, AssignRoleBody body, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new Feature.Command(id, body.RoleName), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .RequireAuthorization(Permissions.UsersManage);
    }

    public record AssignRoleBody(string RoleName);
}
