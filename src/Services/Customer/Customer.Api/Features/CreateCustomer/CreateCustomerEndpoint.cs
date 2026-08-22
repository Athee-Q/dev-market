using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Feature = Customer.Application.Features.CreateCustomer.CreateCustomer;

namespace Customer.Api.Features.CreateCustomer;

/// <summary>
/// Admin-only — regular sign-up creates the Customer row automatically off UserRegisteredEvent
/// (see UserRegisteredConsumer). This exists for an admin adding a customer profile by hand.
/// </summary>
public class CreateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/customers", async (Feature.Command command, IMediator mediator, CancellationToken ct) =>
        {
            var created = await mediator.Send(command, ct);
            return Results.CreatedAtRoute("GetCustomerById", new { id = created.Id }, created);
        })
        .RequireAuthorization(Permissions.CustomersManage);
    }
}
