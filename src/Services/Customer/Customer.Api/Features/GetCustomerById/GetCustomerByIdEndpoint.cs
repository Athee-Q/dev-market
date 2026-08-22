using System.Security.Claims;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Customer.Application.Features.GetCustomerById.GetCustomerById;

namespace Customer.Api.Features.GetCustomerById;

public class GetCustomerByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/customers/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            if (id != user.GetUserId() && !user.HasPermission(Permissions.CustomersManage))
                return Results.Forbid();

            var customer = await mediator.Send(new Feature.Query(id), ct);
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        })
        .WithName("GetCustomerById")
        .RequireAuthorization();
    }
}
