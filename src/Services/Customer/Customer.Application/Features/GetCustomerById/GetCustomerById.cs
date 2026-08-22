using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using Customer.Application.Abstractions;
using Customer.Application.Dto;

namespace Customer.Application.Features.GetCustomerById;

/// <summary>Single customer lookup. Cached 10 min, invalidated explicitly by UpdateCustomer — see ICacheableQuery.
/// Safe to cache the whole response: authorization (id must match the caller, or CustomersManage) happens in
/// GetCustomerByIdEndpoint before the mediator is even invoked, unlike Order's GetOrderById.</summary>
public static class GetCustomerById
{
    public record Query(Guid Id) : IRequest<CustomerDto?>, ICacheableQuery
    {
        public string CacheKey => $"customer:{Id}";
        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }

    public class Handler(ICustomerRepository repository) : IRequestHandler<Query, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(request.Id, cancellationToken);
            return customer is null ? null : CustomerDto.FromDomain(customer);
        }
    }
}
