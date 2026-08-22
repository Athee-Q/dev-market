using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Customer.Application.Features.UpdateCustomer.UpdateCustomer;

namespace Customer.Api.Features.UpdateCustomer;

public record UpdateCustomerBody(string Name, string Email, string Phone);

public class UpdateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/customers/{id:guid}", async (Guid id, UpdateCustomerBody body, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            if (id != user.GetUserId() && !user.HasPermission(Permissions.CustomersManage))
                return Results.Forbid();

            var updated = await mediator.Send(new Feature.Command(id, body.Name, body.Email, body.Phone), ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization();
    }
}
