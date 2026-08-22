using ECommerce.BuildingBlocks.Application.Mediator;
using Customer.Application.Abstractions;
using Customer.Application.Dto;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;

namespace Customer.Application.Features.UpdateCustomer;

public static class UpdateCustomer
{
    public record Command(Guid Id, string Name, string Email, string Phone) : IRequest<CustomerDto?>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        }
    }

    public class Handler(ICustomerRepository repository, IDistributedCache cache) : IRequestHandler<Command, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(Command request, CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (customer is null) return null;

            customer.UpdateProfile(request.Name, request.Email, request.Phone);
            await repository.SaveChangesAsync(cancellationToken);

            // Invalidate rather than update in place — GetCustomerById re-populates the cache on next read.
            await cache.RemoveAsync($"customer:{request.Id}", cancellationToken);

            return CustomerDto.FromDomain(customer);
        }
    }
}
