using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Identity.Application.Features.ListUsers.ListUsers;

namespace Identity.Api.Features.ListUsers;

public class ListUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/users", async (int page, int pageSize, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new Feature.Query(page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(Permissions.UsersManage);
    }
}
